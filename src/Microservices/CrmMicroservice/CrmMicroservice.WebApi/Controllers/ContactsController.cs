using System;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dgt.CrmMicroservice.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContactsController(IMediator mediator)
        {
            _mediator = mediator.WhenNotNull(nameof(mediator));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = (GetContactByIdQuery) id;
            var response = await _mediator.Send(query);
        
            return new OkObjectResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateContactCommand request)
        {
            var response = await _mediator.Send(request);

            return CreatedAtAction(nameof(Get), new {id = response.Id}, null);
        }
    }
}