using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using FluentValidation;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class ValidateRequestPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IValidator<TRequest> _validator;

        public ValidateRequestPipelineBehaviour(IValidator<TRequest> validator)
        {
            _validator = validator.WhenNotNull(nameof(validator));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var result = await _validator.ValidateAsync(request, cancellationToken);
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