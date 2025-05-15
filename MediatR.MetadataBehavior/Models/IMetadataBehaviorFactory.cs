using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.MetadataBehavior.Models
{
    public interface IMetadataBehaviorFactory<TRequest, TResponse> :
       IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
    {
        Task<TResponse>
            MappingBehaviors(
                TRequest request,
                IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
                Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next,
                CancellationToken cancellationToken);
    }
}
