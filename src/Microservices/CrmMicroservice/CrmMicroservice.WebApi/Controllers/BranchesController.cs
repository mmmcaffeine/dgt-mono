using System;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Dgt.CrmMicroservice.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController
    {
        private readonly IBranchRepository _branchRepository;

        // TODO Validate not null
        public BranchesController(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        [HttpGet("{id:guid}")]
        public Task<BranchEntity> Get(Guid id)
        {
            return _branchRepository.GetBranchAsync(id);
        }
    }
}