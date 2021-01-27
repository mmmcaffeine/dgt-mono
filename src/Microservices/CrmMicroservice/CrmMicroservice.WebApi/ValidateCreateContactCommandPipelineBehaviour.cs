using System;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class ValidateCreateContactCommandPipelineBehaviour : IPipelineBehavior<CreateContactCommand, CreateContactResponse>
    {
        private readonly IBranchRepository _branchRepository;

        public ValidateCreateContactCommandPipelineBehaviour(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository.WhenNotNull();
        }

        // TODO Replace with FluentValidation
        public async Task<CreateContactResponse> Handle(CreateContactCommand request, CancellationToken cancellationToken, RequestHandlerDelegate<CreateContactResponse> next)
        {
            if (string.IsNullOrWhiteSpace(request.Title)) throw new InvalidOperationException("Title is missing.");
            if (string.IsNullOrWhiteSpace(request.FirstName)) throw new InvalidOperationException("FirstName is missing.");
            if (string.IsNullOrWhiteSpace(request.LastName)) throw new InvalidOperationException("LastName is missing.");

            try
            {
                _ = await _branchRepository.GetBranchAsync(request.BranchId);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Branch does not exist.", exception);
            }

            return await next();
        }
    }
}