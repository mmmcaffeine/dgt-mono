# The World Before Polly

## The Original Caching Implementation

Prior to experimenting with Polly I had some caching in place for the `FileBasedContactRepository` implementation of `IContactRepository`. This was based on the StackExchange implementation of Redis, and using the `IDistributedCache` interface. I had my own adapter on top of that which was inspired by an idea by Tim Corey.

Redis and `IDistributedCache` does not natively support the Write-Through pattern. I had used the Decorator pattern against the `IContactRepository` interface. This consumed the adapter for `IDistributedCache`. Internally, the decorator was implemented as the Cache Aside pattern i.e. the decorator would check the cache then either return the item from the cache _or_ get the item from the decorated `IContactRepository`, cache it, and return it. This meant consumers of the `IContactRepository` interface could consume it as a Write-Through cache.

The implementation looked something like this (with obvious trimming for brevity):

```csharp
public record Contact(Guid Id, string? Name);

public interface IContactRepository
{
    Task<Contact> GetContactAsync(Guid id);
}

public class ContactRepository : IContactRepository
{
    public Task<Contact> GetContactAsync(Guid id)
    {
        // Read the contact from database, file storage etc
    }
}

public class ContactRepositoryDecorator : IContactRepository
{
    private readonly IContactRepository _repository;
    private readonly IDistributedCache _cache;

    public ContactRepositoryDecorator(IContactRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Contact> GetContactAsync(Guid id)
    {
        // The decorator is implementing this as Cache Aside
        // We'll just pretend the cache works with contacts, not byte arrays
        var contact = await _cache.GetAsync(id.ToString());
        if(contact == null)
        {
            contact = await _repository.GetContactAsync(id);
            await _cache.PutAsync(contact);
        }
        return contact;
    }
}

public class ContactsController
{
    private readonly IContactRepository _repository;

    public ContactsController()
    {
        var toDecorate = new ContactRepository();
        var cache = new RedisCache(); // I'm just making stuff up for illustrative purposes
        _repository = new ContactRepositoryDecorator(toDecorate, cache);
    }

    [HttpGet("{id:guid"})]
    public Task<ActionResult<Contact>> Get(Guid id)
    {
        // From the perspective of the controller this is now acting as a write-through cache.
        return _repository.GetContactAsync(id);
    }
}
```

## The Problem

For the most part this worked very nicely. We typically got a much faster response time. Thanks to the cache we did not hit the artificial delay deliberately built into the code that was reading contacts from disk.

Problems arose when the Redis server was not running. In that scenario it would take a comparatively long time for the `IDistributedCache` to throw an exception. We wouldn't want lack of caching to cause a failure in the API so we would then use the decorated `IContactRepository` as a fallback. We would suffer this delay on every request for a `ContactEntity`. In essence, it was actually possible for the caching mechanism to make the service respond significantly slower!

## The Obvious Solution

The obvious solution to this problem is to implement a circuit breaker. Internally, the decorator could track exceptions thrown when trying to use the `IDistributedCache`. If those exceptions were connectivity problems (such as the server was flat-out uncontactable) it would break the circuit; it would go straight to its fallback (i.e. the decorated `IContactRepository`).

This would require some thought about shared data. If the repositories, decorators, and so forth were declared in transient scope, or even request scope, there would be no knowledge that previous requests had thrown exceptions and that the circuit should be broken. However, that did not seem like an insurmountable problem!

# 🦜 Polly To The Rescue?

## Initial Integration - Adding A Circuit Breaker

[Polly](https://github.com/App-vNext/Polly) has a policy for a circuit breaker out-of-the-box, so was a good place to start. This was fairly simple to integrate into the existing `CachingContactRepositoryDecorator`. My implementation was failing to trip the breaker, but for the reasons already identified i.e. transient scope meaning the state of the breaker was not being persisted. This was fixable, probably through something like a `PolicyRegistry`. Other options like the circuit breaker being static to the decorator would also have worked.

## Next Steps - Replacing The Caching

The next step was to see if I could also utilise Polly's caching policy, rather than rolling my own, and make the two policies play nicely together in a wrap. This is where it all started to fall down...

## A Step Backwards

Having spent some time going through the [documentation for the cache policy](https://github.com/App-vNext/Polly/wiki/Cache) it became apparent that the policy was quite deliberately (and, arguably, reasonably) swallowing any exceptions, and falling back to the non-cached implementation. This meant there was nothing like a `RedisConnectionException` coming back from an `AsyncCachePolicy` that could be used to trip an `AsyncCircuitBreakerPolicy`. Using Polly for the caching would actually put me in a worse position that before; if the Redis server was unavailable I would still have the performance hit of waiting for the exception, but I would no longer be able to have an effective circuit breaker 😢

If I couldn't realistically use Polly for the caching I would have to take the hit of manually implementing caching, and a circuit breaker for anywhere I wanted caching, and all of the additional code that would entail.

## A Horrible Solution

This was such an awful idea that I never tried to prove conclusively that it would actually work...

In theory it would be possibly to:

* Create a circuit breaker policy
* Create a cache policy
  * In the `onCacheError` method execute a delegate through the circuit breaker that simply throws an exception the circuit breaker handles
  * The delegate should also swallow any other exceptions so there are no unhandled exceptions from the hook
* Create a fallback policy
  * It should handle `BrokenCircuitException`
  * It should execute the same delegate you are trying to execute anyway
* Create a wrap of the fallback, circuit breaker, and cache in that order

The theory is when the Redis server is not reachable onGetError would eventually cause the circuit breaker to trip. On the _next_ call the circuit breaker would throw `BrokenCircuitException` which would then cause the fallback policy to execute, rather than the cache policy.

This might have worked, but would have been confusing to set up, and confusing for other developers, so was not a viable solution.

## A Good Solution?

Some experimenting suggested it would be possible to implement my own policy, and plug this into a wrap in exactly the same way as any other policy could be. By favouring composition it seemed I should be able to utilise the Polly `IAsyncCacheProvider` as a layer on top of `IDistributedCache`, and also an `AsyncCircuitBreakerPolicy` internally. I should then be able to come up with an implementation that would have the semantics of both a cache _and_ a circuit breaker.

This would result in the best of all worlds:

* I would be able to get increased performance from caching when the Redis server is available
* I would not take the performance hit of waiting for connections to time out and thrown when the Redis server is not available
* I could re-use all of the existing behaviours for things like cache TTLs and key strategies
* I could re-use all of the existing behaviours for things like circuit breaker resets
* MediatR handlers that wanted caching would not be reliant on hand-rolled write-through caches all the time