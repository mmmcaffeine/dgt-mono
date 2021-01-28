using System;
using System.Reflection;
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
                const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var parameterTypes = new[] {typeof(Exception)};
                var ctor = typeof(TResponse).GetConstructor(bindingFlags, null, parameterTypes, null)!;

                return (TResponse)ctor.Invoke(new object?[] {exception});
            }
        }
    }
}