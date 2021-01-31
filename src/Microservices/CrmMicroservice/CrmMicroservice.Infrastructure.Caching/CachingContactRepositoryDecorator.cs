using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Caching;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;

namespace Dgt.CrmMicroservice.Infrastructure.Caching
{
    public class CachingContactRepositoryDecorator : IContactRepository
    {
        private readonly IContactRepository _contactRepository;
        private readonly ITypedCache _cache;
        private readonly IAsyncPolicy<ContactEntity?> _getContactPolicy;

        public CachingContactRepositoryDecorator(IContactRepository contactRepository, ITypedCache cache)
        {
            _contactRepository = contactRepository.WhenNotNull(nameof(contactRepository));
            _cache = cache.WhenNotNull(nameof(cache));

            // BUG It appears if the repo returns null we execute GetContactFromRepositoryAndCache a second time!
            var exceptionFallbackPolicy = Policy<ContactEntity?>
                .Handle<RedisConnectionException>()
                .Or<BrokenCircuitException>()
                .FallbackAsync(GetContactFromRepositoryAsync, (_, _) => Task.CompletedTask);
            var nullResultFallbackPolicy = Policy<ContactEntity?>
                .HandleResult((ContactEntity?) null)
                .FallbackAsync(GetContactFromRepositoryAndCacheAsync, (_, _) => Task.CompletedTask);
            var circuitBreakerPolicy = Policy<ContactEntity?>
                .Handle<RedisConnectionException>()
                .CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(1000));

            _getContactPolicy = Policy.WrapAsync(nullResultFallbackPolicy, exceptionFallbackPolicy, circuitBreakerPolicy);
        }

        // QUESTION If our repository only ever returns the queryable where would we move caching to?
        public Task<IQueryable<ContactEntity>> GetContactsAsync(CancellationToken cancellationToken = default)
        {
            return _contactRepository.GetContactsAsync(cancellationToken);
        }

        public async Task<ContactEntity?> GetContactAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var key = GetCacheKey(id);
            var context = new Context($"Get Contact {id}")
            {
                {"id", id},
                {"key", key}
            };

            return await _getContactPolicy.ExecuteAsync((_, _) => _cache.GetRecordAsync<ContactEntity>(key), context, cancellationToken);
        }

        private async Task<ContactEntity?> GetContactFromRepositoryAndCacheAsync(Context context, CancellationToken cancellationToken = default)
        {
            var contact = await GetContactFromRepositoryAsync(context, cancellationToken);
            await SafeCacheContact(contact, cancellationToken);
            return contact;
        }

        private async Task<ContactEntity?> GetContactFromRepositoryAsync(Context context, CancellationToken cancellationToken = default)
        {
            return await _contactRepository.GetContactAsync((Guid) context["id"], cancellationToken);
        }

        // ENHANCE You might want some sort of circuit breaker for the cache in here
        public async Task InsertContactAsync([NotNull] ContactEntity contact, CancellationToken cancellationToken = default)
        {
            await _contactRepository.InsertContactAsync(contact, cancellationToken);
            await SafeCacheContact(contact, cancellationToken);
        }

        private async Task SafeCacheContact(ContactEntity? contact, CancellationToken cancellationToken = default)
        {
            if(contact is null) return;
            
            try
            {
                await _cache.SetRecordAsync(GetCacheKey(contact), contact);
            }
            catch
            {
                // REM Deliberately suppress exceptions. We don't want inserting something in the cache to be a failure
            }
        }

        private static string GetCacheKey(ContactEntity contact) => GetCacheKey(contact.Id);
        private static string GetCacheKey(Guid id) => $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
    }
}