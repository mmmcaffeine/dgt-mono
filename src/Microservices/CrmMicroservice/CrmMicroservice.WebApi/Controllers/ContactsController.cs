using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.CrmMicroservice.Domain.Operations.Contacts;
using Dgt.Extensions.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
        public async Task<ActionResult<ContactEntity>> Get(Guid id)
        {
            var query = (GetContactByIdQuery.Request) id;
            var response = await _mediator.Send(query);

            return response.CreateActionResult(this, x => x.Data is null ? NotFound() : Ok(x.Data));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactEntity>>> Get([FromQuery] GetContactsByNameQuery.Request query)
        {
            var response = await _mediator.Send(query);

            return response.CreateActionResult(this, x => Ok(x.Data));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateContactCommand.Request request)
        {
            var response = await _mediator.Send(request);
            var actionResult = response.CreateActionResult(
                this,
                x => CreatedAtAction(nameof(Get), new {Id = x.Data?.CreatedContactId}, null));

            return ((IConvertToActionResult) actionResult).Convert();
        }
    }
}