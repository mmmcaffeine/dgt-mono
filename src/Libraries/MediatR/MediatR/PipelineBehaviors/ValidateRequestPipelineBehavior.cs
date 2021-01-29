using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Dgt.MediatR.PipelineBehaviors
{
    public class ValidateRequestPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidateRequestPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators.WhenNotNull(nameof(validators));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var validationContext = new ValidationContext<TRequest>(request);
            var validationTasks = _validators.Select(validator => validator.ValidateAsync(validationContext, cancellationToken));
            var validationResults = await Task.WhenAll(validationTasks);
            var validationResult = new ValidationResult(validationResults.SelectMany(x => x.Errors));

            if (validationResult.IsValid)
            {
                return await next();
            }

            return IsRichResponse
                ? Response.Failure<TResponse>(validationResult)
                : throw new ValidationException(validationResult.Errors);
        }

        private static bool IsRichResponse => typeof(Response).IsAssignableFrom(typeof(TResponse));
    }
}