using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR.MetadataBehavior.Attributes;
using MediatR.MetadataBehavior.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MetadataBehavior.Models
{
    /// <summary>
    /// 抽象類別，用於實現 Metadata 行為工廠，管理和執行與請求相關的 Metadata 行為。
    /// </summary>
    /// <typeparam name="TRequest">請求型別，必須實現 <see cref="IRequest{TResponse}"/>。</typeparam>
    /// <typeparam name="TResponse">回應型別，表示處理請求後的結果。</typeparam>
    public abstract class MetadataBehaviorFactory<TRequest, TResponse> :
           IMetadataBehaviorFactory<TRequest, TResponse>
           where TRequest : IRequest<TResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 初始化 <see cref="MetadataBehaviorFactory{TRequest, TResponse}"/> 類別的新實例。
        /// </summary>
        /// <param name="serviceProvider">DI 容器，用於解析行為實例。</param>
        public MetadataBehaviorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 處理請求並執行相關的 Metadata 行為。
        /// </summary>
        /// <param name="request">當前的請求物件。</param>
        /// <param name="next">委派，用於執行管道中的下一個行為或處理器。</param>
        /// <param name="cancellationToken">取消操作的通知標記。</param>
        /// <returns>處理請求後的回應結果。</returns>
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // 從 DI 容器中解析所有與請求相關的行為
            var iocBehaviors = _serviceProvider.GetServices<IMetadataBehavior<TRequest, TResponse>>();

            // 取得請求的行為屬性
            var behaviorRequestAttribute = GetBehaviorRequestAttribute(typeof(TRequest));
            var canUseDefault = behaviorRequestAttribute.CanUseDefault == true;

            // 定義行為管道的執行邏輯
            async Task<TResponse> mappingNext(
                IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
                CancellationToken t = default)
            {
                // 如果允許使用默認行為且未找到行為，則使用默認行為
                if (canUseDefault && behaviors == null)
                {
                    behaviors = GetDefaultCustomMediatrBehaviorTypes(behaviorRequestAttribute)
                                    .Select(t => MappingBehavior(t, iocBehaviors));
                    if (behaviors == null || !behaviors.Any())
                        throw new InvalidOperationException($"未找到默認行為。請求型別：{typeof(TRequest).FullName}");
                }

                // 如果沒有行為，直接執行下一個處理器
                if (behaviors == null || !behaviors.Any())
                {
                    var response = await next(cancellationToken);
                    return response;
                }

                // 將行為按順序組合成管道並執行
                var pipelineHandler = behaviors
                            .Reverse()
                            .Aggregate(next, (pnext, pipeline) => c => pipeline.Handle(request, pnext, cancellationToken))();
                return await pipelineHandler;
            }

            // 執行行為映射邏輯
            return await MappingBehaviors(request, iocBehaviors, mappingNext, cancellationToken);
        }

        /// <summary>
        /// 映射並執行 Metadata 行為的邏輯。
        /// </summary>
        /// <param name="request">當前的請求物件。</param>
        /// <param name="behaviors">與請求相關的 Metadata 行為集合。</param>
        /// <param name="next">委派，用於執行管道中的下一個行為或處理器。</param>
        /// <param name="cancellationToken">取消操作的通知標記。</param>
        /// <returns>處理請求後的回應結果。</returns>
        public abstract Task<TResponse> MappingBehaviors(
            TRequest request,
            IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
            Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken);

        /// <summary>
        /// 取得默認的行為型別集合。
        /// </summary>
        /// <param name="behaviorRequestAttribute">行為請求屬性。</param>
        /// <returns>默認行為型別集合。</returns>
        private IEnumerable<Type> GetDefaultCustomMediatrBehaviorTypes(RequestAttribute behaviorRequestAttribute)
        {
            (Type request, Type response) = typeof(TRequest).GetTypeInfo().GetRequestAndResponseType();
            var metadataType = behaviorRequestAttribute?.MetadataType ?? request;

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

        /// <summary>
        /// 映射行為型別到具體的行為實例。
        /// </summary>
        /// <param name="behaviorType">行為型別。</param>
        /// <param name="behaviors">行為實例集合。</param>
        /// <returns>對應的行為實例。</returns>
        /// <exception cref="InvalidOperationException">當未找到匹配的行為型別時拋出。</exception>
        private IMetadataBehavior<TRequest, TResponse> MappingBehavior(
            Type behaviorType,
            IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors)
        {
            var behavior = behaviors.FirstOrDefault(b => b.GetType() == behaviorType);
            if (behavior == null)
                throw new InvalidOperationException($"未找到匹配的行為型別：{behaviorType}");
            return behavior;
        }

        /// <summary>
        /// 取得請求型別上的行為屬性。
        /// </summary>
        /// <param name="request">請求型別。</param>
        /// <returns>行為請求屬性。</returns>
        private RequestAttribute GetBehaviorRequestAttribute(Type request)
        {
            return request.GetCustomAttributes(false)
                                .Where(att => att.GetType().IsAssignableTo(typeof(RequestAttribute)))
                                .Select(att => att as RequestAttribute)
                                .SingleOrDefault();
        }

        /// <summary>
        /// 取得行為型別的名稱。
        /// </summary>
        /// <param name="type">行為型別。</param>
        /// <returns>行為型別的名稱。</returns>
        private string GetBehaviorName(Type type)
        {
            if (type.IsGenericType)
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}";
            else
                return type.Name;
        }
    }
}
