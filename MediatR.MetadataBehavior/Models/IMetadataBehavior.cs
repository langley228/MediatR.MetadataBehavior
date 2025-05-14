using System.Threading;
using System.Threading.Tasks;

namespace MediatR.PipelineExtensions.Models
{
    /// <summary>
    /// 自訂流程 Behavior 的介面
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IMetadataBehavior<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken);
    }
}
