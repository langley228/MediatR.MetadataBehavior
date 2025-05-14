using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR.PipelineExtensions.Attributes;
using MediatR.PipelineExtensions.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.PipelineExtensions.Models
{
    public abstract class MetadataBehaviorFactory<TRequest, TResponse> :
           IMetadataBehaviorFactory<TRequest, TResponse>
           where TRequest : IRequest<TResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        public MetadataBehaviorFactory(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var iocBehaviors = _serviceProvider
                            .GetServices<IMetadataBehavior<TRequest, TResponse>>();
            var behaviorRequestAttribute = GetBehaviorRequestAttribute(typeof(TRequest));
            var canUseDefault = behaviorRequestAttribute.CanUseDefault == true;
            async Task<TResponse> mappingNext(
                IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
                CancellationToken t = default)
            {
                if (canUseDefault && behaviors == null)
                {
                    behaviors = GetDefaultCustomMediatrBehaviorTypes(behaviorRequestAttribute)
                                    .Select(t => MappingBehavior(t, iocBehaviors));
                    if (behaviors == null ||
                        !behaviors.Any())
                        throw new InvalidOperationException($"not found default custom behavior. request type: {typeof(TRequest).FullName}");
                }


                if (behaviors == null ||
                    !behaviors.Any())
                {
                    var response = await next(cancellationToken);
                    return response;
                }

                /*
                 參考
                https://github.com/jbogard/MediatR/blob/master/src/MediatR/Wrappers/RequestHandlerWrapper.cs#L48
                 */
                var pipelineHandler = behaviors
                            .Reverse()
                            .Aggregate(next, (pnext, pipeline) => c => pipeline.Handle(request, pnext, cancellationToken))();
                return await pipelineHandler;
            }
            ;
            return await MappingBehaviors(request, iocBehaviors, mappingNext, cancellationToken);
        }

        public abstract
            Task<TResponse>
            MappingBehaviors(
                TRequest request,
                IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
                Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next,
                CancellationToken cancellationToken);

        private IEnumerable<Type> GetDefaultCustomMediatrBehaviorTypes(RequestAttribute behaviorRequestAttribute)
        {
            (Type request, Type response) = typeof(TRequest).GetTypeInfo().GetRequestAndResponseType();
            var metadataType = behaviorRequestAttribute?.MetadataType;
            if (metadataType == null)
                metadataType = request;
            if (metadataType == null)
                return null;


            var attrs = metadataType.GetCustomAttributes(false)
                                .Where(att => att.GetType().IsAssignableTo(typeof(MetadataAttribute)))
                                .Select(att => att as MetadataAttribute)
                                .ToList();

            var behaviorTypes = attrs
                                .Where(s => s.IsDefault)
                                .OrderBy(s => s.Order)
                                .Select(s => s.BehaviorGenericType.MakeMetadataBehaviorType(request, response, s.GenericTypeParameters))
                                .ToList();

            return behaviorTypes;
        }

        private IMetadataBehavior<TRequest, TResponse> MappingBehavior(
            Type behaviorType,
            IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors)
        {
            var behavior = behaviors.FirstOrDefault(b => b.GetType() == behaviorType);
            if (behavior == null)
                throw new InvalidOperationException($"No matching behavior Type. behavior : {behaviorType}");
            return behavior;
        }

        private RequestAttribute GetBehaviorRequestAttribute(Type request)
        {
            return request.GetCustomAttributes(false)
                                .Where(att => att.GetType().IsAssignableTo(typeof(RequestAttribute)))
                                .Select(att => att as RequestAttribute)
                                .SingleOrDefault();
        }

        private string GetBehaviorName(Type type)
        {
            if (type.IsGenericType)
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}";
            else
                return type.Name;
        }
    }
}
