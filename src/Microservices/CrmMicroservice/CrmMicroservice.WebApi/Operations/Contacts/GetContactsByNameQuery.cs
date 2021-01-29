using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using FluentValidation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi.Operations.Contacts
{
    public sealed class GetContactsByNameQuery
    {
        private enum Condition
        {
            And,
            Or
        }

        // TODO Condition must be a member of the enumeration
        // TODO Honour the Condition property
        // TODO Include boolean for case sensitivity
        public class Request : IRequest<Response<IEnumerable<ResponseData>>>
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public bool Partial { get; set; } = false;
            public string? Condition { get; set; } = GetContactsByNameQuery.Condition.And.ToString();
        }

        public class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(request => request.FirstName)
                    .NotEmpty()
                    .When(request => string.IsNullOrWhiteSpace(request.LastName))
                    .WithMessage($"{{PropertyName}} must be supplied when {nameof(Request.LastName)} is not.");
                RuleFor(request => request.LastName)
                    .NotEmpty()
                    .When(request => string.IsNullOrWhiteSpace(request.FirstName))
                    .WithMessage($"{{PropertyName}} must be supplied when {nameof(Request.FirstName)} is not.");
            }
        }

        // TODO This is the same as in GetContactByIdQuery. Remove the duplication
        public class ResponseData
        {
            public Guid Id { get; init; }
            public string? Title { get; init; }
            public string? FirstName { get; init; }
            public string? LastName { get; init; }
            public Guid BranchId { get; init; }
        }

        public class Handler : IRequestHandler<Request, Response<IEnumerable<ResponseData>>>
        {
            private readonly IContactRepository _contactRepository;

            public Handler(IContactRepository contactRepository)
            {
                _contactRepository = contactRepository;
            }

            // REM This could be a problem. When working with an IQueryable you don't know if the underlying type will
            //     support the expression tree you are building up.
            public async Task<Response<IEnumerable<ResponseData>>> Handle(Request request, CancellationToken cancellationToken)
            {
                var contacts = await _contactRepository.GetContactsAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(request.FirstName))
                {
                    contacts = request.Partial
                        ? contacts.Where(contact => !string.IsNullOrEmpty(contact.FirstName) &&  contact.FirstName.Contains(request.FirstName))
                        : contacts.Where(contact => string.Equals(contact.FirstName, request.FirstName)); 
                }

                if (!string.IsNullOrEmpty(request.LastName))
                {
                    contacts = request.Partial
                        ? contacts.Where(contact => !string.IsNullOrEmpty(contact.LastName) &&  contact.LastName.Contains(request.LastName))
                        : contacts.Where(contact => string.Equals(contact.LastName, request.LastName));
                }

                return Response.Success(contacts.Select(contact => CreateResponseData(contact)).AsEnumerable());
            }

            private static ResponseData CreateResponseData(ContactEntity contact)
            {
                return new()
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
}