using System;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using FluentValidation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi.Operations.Contacts
{
    public sealed class CreateContactCommand
    {
        public class Request : IRequest<Response<ResponseData>>
        {
            public string? Title { get; init; }
            public string? FirstName { get; init; }
            public string? LastName { get; init; }
            public Guid BranchId { get; init; }
        }

        public class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator(IBranchRepository branchRepository)
            {
                _ = branchRepository.WhenNotNull();

                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.FirstName).NotEmpty();
                RuleFor(x => x.LastName).NotEmpty();
                RuleFor(x => x.BranchId).MustAsync(async (branchId, token) =>
                {
                    try
                    {
                        _ = await branchRepository.GetBranchAsync(branchId);

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }).WithMessage("Branch does not exist.");
            }
        }

        public class ResponseData
        {
            public Guid CreatedContactId { get; init; }
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
                var contact = new ContactEntity
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BranchId = request.BranchId
                };

                await _contactRepository.InsertContactAsync(contact, cancellationToken);

                var data = new ResponseData {CreatedContactId = contact.Id};

                return Response.Success(data);
            }
        }
    }
}