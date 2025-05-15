using System.Threading;
using System.Threading.Tasks;

namespace MediatR.MetadataBehavior.Models
{
    /// <summary>
    /// 定義自訂流程行為的介面，用於在 MediatR 的處理管道中插入自訂邏輯。
    /// </summary>
    /// <typeparam name="TRequest">請求型別，必須實現 <see cref="IRequest{TResponse}"/>。</typeparam>
    /// <typeparam name="TResponse">回應型別，表示處理請求後的結果。</typeparam>
    public interface IMetadataBehavior<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// 處理請求的行為邏輯。
        /// </summary>
        /// <param name="request">當前的請求物件。</param>
        /// <param name="next">委派，用於執行管道中的下一個行為或處理器。</param>
        /// <param name="cancellationToken">取消操作的通知標記。</param>
        /// <returns>處理請求後的回應結果。</returns>
        Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken);
    }
}
