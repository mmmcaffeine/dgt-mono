using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class ValidateRequestPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IValidator<TRequest>? _validator;

        public ValidateRequestPipelineBehaviour() : this(null)
        {
        }

        public ValidateRequestPipelineBehaviour(IValidator<TRequest>? validator)
        {
            _validator = validator;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var validationResult = _validator != null
                ? await _validator.ValidateAsync(request, cancellationToken)
                : new ValidationResult();

            if (validationResult.IsValid)
            {
                return await next();
            }

            if (!IsRichResponse)
            {
                var exceptions = validationResult.Errors.Select(error => new InvalidOperationException(error.ErrorMessage)).ToList();
                throw exceptions.Count == 1
                    ? exceptions.Single()
                    : new AggregateException("The request failed validation.", exceptions);
            }

            return Response.Failure<TResponse>(validationResult);
        }

        private static bool IsRichResponse => typeof(Response).IsAssignableFrom(typeof(TResponse));
    }
}