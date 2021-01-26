using System;
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
    }
}