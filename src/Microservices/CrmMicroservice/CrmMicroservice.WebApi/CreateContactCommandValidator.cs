using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using FluentValidation;

namespace Dgt.CrmMicroservice.WebApi
{
    public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
    {
        public CreateContactCommandValidator(IBranchRepository branchRepository)
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
}