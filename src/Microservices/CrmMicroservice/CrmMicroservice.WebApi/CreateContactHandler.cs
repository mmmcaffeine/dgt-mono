using System;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class CreateContactHandler : IRequestHandler<CreateContactCommand, Response<CreateContactResponse>>
    {
        private readonly IContactRepository _contactRepository;

        public CreateContactHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository.WhenNotNull(nameof(contactRepository));
        }

        public async Task<Response<CreateContactResponse>> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            var contact = new ContactEntity
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BranchId = request.BranchId
            };

            await _contactRepository.InsertContactAsync(contact, cancellationToken);

            var data = new CreateContactResponse {Id = contact.Id};

            return Response.Success(data);
        }
    }
}