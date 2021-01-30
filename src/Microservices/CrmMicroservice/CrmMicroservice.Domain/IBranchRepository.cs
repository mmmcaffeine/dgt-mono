using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dgt.CrmMicroservice.Domain
{
    public interface IBranchRepository
    {
        Task<IQueryable<BranchEntity>> GetBranchesAsync(CancellationToken cancellationToken = default);
    }
}