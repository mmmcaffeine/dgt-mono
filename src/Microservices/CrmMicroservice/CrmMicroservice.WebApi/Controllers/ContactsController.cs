using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.WebApi.Operations.Contacts;
using Dgt.Extensions.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            var query = (GetContactByIdQuery.Request) id;
            var response = await _mediator.Send(query);
        
            return new OkObjectResult(response.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateContactCommand.Request request)
        {
            var response = await _mediator.Send(request);

            // TODO Can I tidy this up with a switch expression when it gets moved to e.g. an action filter?
            if (response.Successful)
            {
                return CreatedAtAction(nameof(Get), new {id = response.Data?.CreatedContactId}, null);
            }
            else if (response.Exception is not null)
            {
                return StatusCode((int) HttpStatusCode.InternalServerError, response.Exception.Message);
            }
            else if (response.ValidationResult is not null && response.ValidationResult.Errors.Any())
            {
                var dictionary = new ModelStateDictionary();

                foreach (var error in response.ValidationResult.Errors)
                {
                    dictionary.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return ValidationProblem(dictionary);
            }
            else
            {
                // Not successful, but there is either no exception, or no validation errors... WTF?
                return StatusCode((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}