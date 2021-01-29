using System;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.CrmMicroservice.Domain.Operations.Branches;
using Dgt.Extensions.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dgt.CrmMicroservice.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BranchesController(IMediator mediator)
        {
            _mediator = mediator.WhenNotNull(nameof(mediator));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BranchEntity>> Get(Guid id)
        {
            var request = new GetBranchByIdQuery.Request(id);
            var response = await _mediator.Send(request);

            return response.CreateActionResult(this, x => x.Data is null ? NotFound() : Ok(x.Data));
        }
    }
}