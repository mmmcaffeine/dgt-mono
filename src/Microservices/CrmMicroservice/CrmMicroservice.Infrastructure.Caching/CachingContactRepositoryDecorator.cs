using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Caching;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;

namespace Dgt.CrmMicroservice.Infrastructure.Caching
{
    public class CachingContactRepositoryDecorator : IContactRepository
    {
        private readonly IContactRepository _contactRepository;
        private readonly ITypedCache _cache;

        public CachingContactRepositoryDecorator(IContactRepository contactRepository, ITypedCache cache)
        {
            _contactRepository = contactRepository.WhenNotNull(nameof(contactRepository));
            _cache = cache.WhenNotNull(nameof(cache));
        }

        // QUESTION If our repository only ever returns the queryable where would we move caching to?
        public Task<IQueryable<ContactEntity>> GetContactsAsync(CancellationToken cancellationToken = default)
        {
            return _contactRepository.GetContactsAsync(cancellationToken);
        }

        // ENHANCE You might want some sort of circuit breaker for the cache in here
        public async Task<ContactEntity> GetContactAsync(Guid id)
        {
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            ContactEntity? contact; 

            try
            {
                contact = await _cache.GetRecordAsync<ContactEntity>(key);
            }
            catch
            {
                return await _contactRepository.GetContactAsync(id);
            }

            if (contact is null)
            {
                contact = await _contactRepository.GetContactAsync(id);
                await _cache.SetRecordAsync(key, contact);
            }

            return contact;
        }

        public async Task InsertContactAsync([NotNull] ContactEntity contact, CancellationToken cancellationToken = default)
        {
            var key = $"{nameof(ContactEntity)}:{contact.Id}".ToLowerInvariant();

            await _contactRepository.InsertContactAsync(contact, cancellationToken);
            await _cache.SetRecordAsync(key, contact);
        }
    }
}