using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.MetadataBehavior.Models;

namespace MediatR.MetadataBehavior.Sample.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IMetadataBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[Validation] Validating request: {typeof(TRequest).Name}");
            if (request is Models.CustomRequest sampleRequest &&
                (sampleRequest.BehaviorNames == null ||
                sampleRequest.BehaviorNames.Length == 0))
            {
                throw new ArgumentException("BehaviorNames cannot be null or empty.");
            }
            return await next();
        }
    }
}