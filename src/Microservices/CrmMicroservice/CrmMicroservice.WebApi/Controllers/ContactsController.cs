using System;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Dgt.CrmMicroservice.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;

        // TODO Validate not null
        public ContactsController(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [HttpGet("{id:guid}")]
        public Task<ContactEntity> Get(Guid id)
        {
            return _contactRepository.GetContactAsync(id);
        }
    }
}