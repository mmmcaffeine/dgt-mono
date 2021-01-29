using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dgt.CrmMicroservice.Domain
{
    public interface IBranchRepository
    {
        [Obsolete("All operations are being moved over to MediatR handlers.")]
        Task<BranchEntity> GetBranchAsync(Guid id);
        Task<IQueryable<BranchEntity>> GetBranchesAsync(CancellationToken cancellationToken = default);
    }
}