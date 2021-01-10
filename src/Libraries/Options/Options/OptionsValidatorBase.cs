using FluentValidation;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Dgt.Options
{
    public abstract class OptionsValidatorBase<TOptions> : AbstractValidator<TOptions>, IValidateOptions<TOptions>
        where TOptions : class
    {
        public ValidateOptionsResult Validate(string name, TOptions options)
        {
            var result = Validate(options);

            return result.IsValid
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(result.Errors.Select(error => error.ErrorMessage));
        }
    }
}