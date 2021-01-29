using System;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using Dgt.MediatR;
using MediatR;

namespace Dgt.CrmMicroservice.Domain.Operations.Contacts
{
    public sealed class GetContactByIdQuery
    {
        public class Request : IRequest<Response<ContactEntity>>
        {
            public Guid Id { get; init; }

            public static implicit operator Request(Guid id) => new Request {Id = id};
            public static implicit operator Guid(Request? request) => request?.Id ?? Guid.Empty;
        }

        public class Handler : IRequestHandler<Request, Response<ContactEntity>>
        {
            private readonly IContactRepository _contactRepository;

            public Handler(IContactRepository contactRepository)
            {
                _contactRepository = contactRepository.WhenNotNull(nameof(contactRepository));
            }

            public async Task<Response<ContactEntity>> Handle(Request request, CancellationToken cancellationToken)
            {
                ContactEntity contact;

                try
                {
                    contact = await _contactRepository.GetContactAsync(request.Id);
                }
                catch (ArgumentException exception) when (exception.Message.StartsWith("No entity with the supplied ID exists."))
                {
                    return Response.Empty<ContactEntity>();
                }

                return Response.Success(contact);
            }
        }
    }
}