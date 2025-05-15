using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.MetadataBehavior.Models
{
    /// <summary>
    /// 預設的 Metadata 行為工廠，當沒有自訂邏輯時使用。
    /// </summary>
    /// <typeparam name="TRequest">請求型別，必須實現 <see cref="IRequest{TResponse}"/>。</typeparam>
    /// <typeparam name="TResponse">回應型別，表示處理請求後的結果。</typeparam>
    public class DefaultMetadataBehaviorFactory<TRequest, TResponse> :
        MetadataBehaviorFactory<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// 初始化 <see cref="DefaultMetadataBehaviorFactory{TRequest, TResponse}"/> 類別的新實例。
        /// </summary>
        /// <param name="serviceProvider">DI 容器，用於解析行為實例。</param>
        public DefaultMetadataBehaviorFactory(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// 映射並執行 Metadata 行為的邏輯。
        /// </summary>
        /// <param name="request">當前的請求物件。</param>
        /// <param name="behaviors">與請求相關的 Metadata 行為集合。</param>
        /// <param name="next">委派，用於執行管道中的下一個行為或處理器。</param>
        /// <param name="cancellationToken">取消操作的通知標記。</param>
        /// <returns>處理請求後的回應結果。</returns>
        public override Task<TResponse> MappingBehaviors(
            TRequest request,
            IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
            Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken)
        {
            // 預設行為工廠不執行任何行為，直接調用下一個處理器
            return next(null, cancellationToken);
        }
    }
}
