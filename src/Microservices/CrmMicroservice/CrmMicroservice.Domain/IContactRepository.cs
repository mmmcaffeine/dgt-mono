using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dgt.CrmMicroservice.Domain
{
    public interface IContactRepository
    {
        // QUESTION How do we want to expose this in a Clean Architecture. Ideally we want people to be able to
        //          work in an async manner, but we don't want to tie ourselves into something like the async
        //          extensions in Entity Framework. Can we even make Clean Architecture and Vertical Slice
        //          architecture play nicely together?
        Task<IQueryable<ContactEntity>> GetContactsAsync(CancellationToken cancellationToken = default);
        Task<ContactEntity> GetContactAsync(Guid id);
        Task InsertContactAsync([NotNull] ContactEntity contact, CancellationToken cancellationToken = default);
    }
}