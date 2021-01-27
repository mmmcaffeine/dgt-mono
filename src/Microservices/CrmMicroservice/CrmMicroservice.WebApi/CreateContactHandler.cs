using System;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class CreateContactHandler : IRequestHandler<CreateContactCommand, CreateContactResponse>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IContactRepository _contactRepository;

        public CreateContactHandler(IBranchRepository branchRepository, IContactRepository contactRepository)
        {
            _branchRepository = branchRepository.WhenNotNull(nameof(branchRepository));
            _contactRepository = contactRepository.WhenNotNull(nameof(contactRepository));
        }

        // TODO Pipeline validation of the incoming request i.e. FirstName must be not empty etc
        public async Task<CreateContactResponse> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _ = await _branchRepository.GetBranchAsync(request.BranchId);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("The indicated branch does not exist", exception);
            }

            var contact = new ContactEntity
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BranchId = request.BranchId
            };

            await _contactRepository.InsertContactAsync(contact, cancellationToken);

            // TODO How to get the location without hard-wiring the path? Maybe we only return the Id, and let the controller handle that
            return new CreateContactResponse
            {
                Id = contact.Id,
                Uri = $"contacts/{contact.Id}"
            };
        }
    }
}