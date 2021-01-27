using System;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dgt.CrmMicroservice.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactsController : ControllerBase
    {
        private const string GetContactById = "GetContactById";
        private const string InsertContact = "InsertContact";
        
        private readonly IContactRepository _contactRepository;
        private readonly IMediator _mediator;

        public ContactsController(IContactRepository contactRepository, IMediator mediator)
        {
            _contactRepository = contactRepository.WhenNotNull(nameof(mediator));
            _mediator = mediator.WhenNotNull(nameof(mediator));
        }

        [HttpGet("{id:guid}", Name = GetContactById)]
        public Task<ContactEntity> Get(Guid id)
        {
            return _contactRepository.GetContactAsync(id);
        }

        [HttpPost(Name = InsertContact)]
        public async Task<IActionResult> Post([FromBody] CreateContactCommand request)
        {
            var response = await _mediator.Send(request);

            return CreatedAtRoute(GetContactById, new {id = response.Id}, null);
        }
    }
}