using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.MetadataBehavior.Models
{

    public class DefaultMetadataBehaviorFactory<TRequest, TResponse> :
        MetadataBehaviorFactory<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public DefaultMetadataBehaviorFactory(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override Task<TResponse> MappingBehaviors(
            TRequest request,
            IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
            Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
        {
            return next(null, cancellationToken);
        }
    }
}
