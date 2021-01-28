using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, GetContactByIdResponse>
    {
        private readonly IContactRepository _contactRepository;

        public GetContactByIdQueryHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository.WhenNotNull(nameof(contactRepository));
        }

        public async Task<GetContactByIdResponse> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
        {
            var contact = await _contactRepository.GetContactAsync(request.Id);

            return new GetContactByIdResponse
            {
                Id = contact.Id,
                Title = contact.Title,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                BranchId = contact.BranchId
            };
        }
    }
}