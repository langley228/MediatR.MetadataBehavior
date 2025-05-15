using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.MetadataBehavior.Models;

namespace MediatR.MetadataBehavior.Sample.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IMetadataBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[Logging] Handling request: {typeof(TRequest).Name}");
            var response = await next();
            Console.WriteLine($"[Logging] Finished handling request: {typeof(TRequest).Name}");
            return response;
        }
    }
}