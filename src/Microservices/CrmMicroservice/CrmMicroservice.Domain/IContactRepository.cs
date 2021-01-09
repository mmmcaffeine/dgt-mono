using System;
using System.Threading.Tasks;

namespace Dgt.CrmMicroservice.Domain
{
    public interface IContactRepository
    {
        Task<ContactEntity> GetContactAsync(Guid id);
    }
}