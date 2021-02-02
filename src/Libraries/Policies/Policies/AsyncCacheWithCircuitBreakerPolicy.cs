using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using Polly;
using Polly.Caching;
using Polly.CircuitBreaker;

namespace Dgt.Policies
{
    // AsyncCircuitBreakerPolicy<TResult> implements the following interfaces which I should also implement
    // ICircuitBreakerPolicy<T> (implemented by AsyncCircuitBreakerPolicy<TResult>)
    // ICircuitBreakerPolicy (implemented by syncCircuitBreakerPolicy<TResult>)
    // IsPolicy (marker interface implemented by AsyncCircuitBreakerPolicy<TResult>)

    // AsyncCachePolicy<TResult> does not implement any interfaces so nothing else I need to implement

    // Both of these types inherit from AsyncPolicy<TResult>
    public class AsyncCacheWithCircuitBreakerPolicy<TResult> : AsyncPolicy<TResult>
    {
        private readonly IAsyncCacheProvider<TResult> _asyncCacheProvider;
        private readonly ITtlStrategy<TResult> _ttlStrategy;
        private readonly ICacheKeyStrategy _cacheKeyStrategy;

        // TODO We obviously need the configuration for this being passed in, and not hard-coded
        private readonly AsyncCircuitBreakerPolicy<TResult> _circuitBreakerPolicy = Policy<TResult>
            .Handle<Exception>()
            .CircuitBreakerAsync(1, TimeSpan.MaxValue);

        // Why do I not need a constructor now? Was I inheriting from PolicyBase before, rather than AsyncPolicy<TResult>?
        // Polly implementations seem to hold Func<Context, string> for the cache key strategy
        // You should have an overload the accepts the default cache key strategy
        // TODO The typical way of working in Polly seems to be internal constructors, and created with a fluent syntax
        public AsyncCacheWithCircuitBreakerPolicy(
            IAsyncCacheProvider<TResult> asyncCacheProvider,
            ITtlStrategy<TResult> ttlStrategy,
            ICacheKeyStrategy cacheKeyStrategy
        )
        {
            _asyncCacheProvider = asyncCacheProvider.WhenNotNull(nameof(asyncCacheProvider));
            _ttlStrategy = ttlStrategy.WhenNotNull(nameof(ttlStrategy));
            _cacheKeyStrategy = cacheKeyStrategy.WhenNotNull(nameof(cacheKeyStrategy));
        }

        // TODO Review AsyncCacheEngine to ensure we have the same semantics, particularly around hooks
        protected override async Task<TResult> ImplementationAsync(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            // TODO Check you are handling the correct states here
            // What about e.g. HalfOpen? Should I be executing via the breaker then, and have I broken its semantics
            // if I don't? Can I do better by using a policy wrap with a fallback, and then always executing via the
            // circuit breaker? The fallback would need some of these parameters so we might need to create a new fallback
            // and wrap on every call. That could end up being inefficient, but might be safer in terms of not subtly
            // changing circuit breaker behaviours I'm not aware of
            if (_circuitBreakerPolicy.CircuitState != CircuitState.Closed)
            {
                return await action(context, cancellationToken);
            }

            var policyResult = await _circuitBreakerPolicy.ExecuteAndCaptureAsync(
                (c, t) => DoGetValue(action, c, t, continueOnCapturedContext),
                context,
                cancellationToken,
                continueOnCapturedContext);

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                return policyResult.Result;
            }

            return await action(context, cancellationToken);
        }

        // TODO This needs quite a bit of work...
        // If we didn't get the value from the cache, but successfully called the delegate, but _then_ we failed putting
        // the value _into_ the cache we shouldn't let that stop us giving the returned value back. We wouldn't
        // want to call the delegate a second time. It is questionable whether we'd want that second failure to also
        // contribute towards tripping the breaker. It seems likely that if the call the Put is going to fail the call
        // to Get would also have done beforehand, but we can't guarantee that. For example, we might just have a flaky
        // connection to a Redis server. It could also do with a better name.
        [SuppressMessage("ReSharper", "CA1068", Justification = "Method signature selected to match protected method inherited from AsyncPolicy")]
        private async Task<TResult> DoGetValue(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            var key = _cacheKeyStrategy.GetCacheKey(context);

            var (cacheHit, valueFromCache) = await _asyncCacheProvider.TryGetAsync(key, cancellationToken, continueOnCapturedContext);

            if (cacheHit)
            {
                return valueFromCache;
            }

            // TODO How would we stop failures here contributing to the breaker? The breaker should _only_ be for the cache
            valueFromCache = await action(context, cancellationToken);

            var ttl = _ttlStrategy.GetTtl(context, valueFromCache);
            await _asyncCacheProvider.PutAsync(key, valueFromCache, ttl, cancellationToken, continueOnCapturedContext);

            return valueFromCache;
        }
    }
}