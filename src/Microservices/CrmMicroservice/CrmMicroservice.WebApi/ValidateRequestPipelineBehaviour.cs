using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
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
            if (_validator is null)
            {
                return await next();
            }

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