using System;
using System.Threading.Tasks;

namespace Dgt.CrmMicroservice.Domain
{
    public interface IBranchRepository
    {
        Task<BranchEntity> GetBranchAsync(Guid id);
    }
}