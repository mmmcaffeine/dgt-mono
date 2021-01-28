using System;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi.Operations.Contacts
{
    public sealed class GetContactByIdQuery
    {
        public class Request : IRequest<Response<ResponseData>>
        {
            public Guid Id { get; init; }

            public static implicit operator Request(Guid id) => new Request {Id = id};
            public static implicit operator Guid(Request? request) => request?.Id ?? Guid.Empty;
        }

        public class ResponseData
        {
            public Guid Id { get; init; }
            public string? Title { get; init; }
            public string? FirstName { get; init; }
            public string? LastName { get; init; }
            public Guid BranchId { get; init; }
        }

        public class Handler : IRequestHandler<Request, Response<ResponseData>>
        {
            private readonly IContactRepository _contactRepository;

            public Handler(IContactRepository contactRepository)
            {
                _contactRepository = contactRepository.WhenNotNull(nameof(contactRepository));
            }

            public async Task<Response<ResponseData>> Handle(Request request, CancellationToken cancellationToken)
            {
                var contact = await _contactRepository.GetContactAsync(request.Id);
                var data = new ResponseData
                {
                    Id = contact.Id,
                    Title = contact.Title,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    BranchId = contact.BranchId
                };

                return Response.Success(data);
            }
        }
    }
}