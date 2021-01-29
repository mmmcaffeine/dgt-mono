using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Dgt.CrmMicroservice.Domain.Operations.Contacts
{
    // REM This deliberately isn't implemented "properly" because it only exists to explore the behaviour of
    //     our IActionFilter for validation of requests
    public sealed class DeleteContactByIdCommand
    {
        public record Request(Guid Id) : IRequest<Unit>;

        public class RequestValidator : AbstractValidator<Request>
        {
            public RequestValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public class DeleteContactByIdHandler : IRequestHandler<Request, Unit>
        {
            public Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
        }
    }
}