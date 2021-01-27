using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Dgt.CrmMicroservice.Domain
{
    public interface IContactRepository
    {
        Task<ContactEntity> GetContactAsync(Guid id);
        Task InsertContactAsync([NotNull] ContactEntity contact, CancellationToken cancellationToken = default);
    }
}