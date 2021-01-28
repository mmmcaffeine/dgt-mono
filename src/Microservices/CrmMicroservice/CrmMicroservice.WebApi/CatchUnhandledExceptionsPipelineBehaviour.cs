using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class CatchUnhandledExceptionsPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : Response
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception exception)
            {
                var response = Activator.CreateInstance<TResponse>();
                response.Exception = exception;
                return response;
            }
        }
    }
}