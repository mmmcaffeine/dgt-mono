using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using FluentValidation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class ValidateCreateContactCommandPipelineBehaviour
        : AbstractValidator<CreateContactCommand>, IPipelineBehavior<CreateContactCommand, CreateContactResponse>
    {
        public ValidateCreateContactCommandPipelineBehaviour(IBranchRepository branchRepository)
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

        public async Task<CreateContactResponse> Handle(
            CreateContactCommand request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<CreateContactResponse> next)
        {
            var result = await ValidateAsync(request, cancellationToken);
            var exceptions = result.Errors.Select(error => new InvalidOperationException(error.ErrorMessage)).ToList();
            
            return exceptions.Count switch
            {
                0 => await next(),
                1 => throw exceptions.Single(),
                _ => throw new AggregateException("The request failed validation.", exceptions)
            };
        }
    }
}