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

        public async Task<ContactEntity> GetContactAsync(Guid id)
        {
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var contact = await _cache.GetRecordAsync<ContactEntity>(key);

            if (contact is null)
            {
                contact = await _contactRepository.GetContactAsync(id);
                await _cache.SetRecordAsync(key, contact);
            }

            return contact;
        }
    }
}