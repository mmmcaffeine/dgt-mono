using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Dgt.MediatR.PipelineBehaviors
{
    public class CatchUnhandledExceptionsPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
                return Response.Failure<TResponse>(exception);
            }
        }
    }
}