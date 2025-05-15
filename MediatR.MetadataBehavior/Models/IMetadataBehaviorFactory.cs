using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.MetadataBehavior.Models
{
    /// <summary>
    /// 定義 Metadata 行為工廠的介面，用於管理和執行多個 Metadata 行為。
    /// </summary>
    /// <typeparam name="TRequest">請求型別，必須實現 <see cref="IRequest{TResponse}"/>。</typeparam>
    /// <typeparam name="TResponse">回應型別，表示處理請求後的結果。</typeparam>
    public interface IMetadataBehaviorFactory<TRequest, TResponse> :
       IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// 映射並執行 Metadata 行為的邏輯。
        /// </summary>
        /// <param name="request">當前的請求物件。</param>
        /// <param name="behaviors">與請求相關的 Metadata 行為集合。</param>
        /// <param name="next">委派，用於執行管道中的下一個行為或處理器。</param>
        /// <param name="cancellationToken">取消操作的通知標記。</param>
        /// <returns>處理請求後的回應結果。</returns>
        Task<TResponse> MappingBehaviors(
            TRequest request,
            IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
            Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken);
    }
}
