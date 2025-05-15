using System.Threading;
using System.Threading.Tasks;
using MediatR.MetadataBehavior.Sample.Models;

namespace MediatR.MetadataBehavior.Sample.Handlers
{
    public class DefaultRequestHandler : IRequestHandler<DefaultRequest, string>
    {
        public Task<string> Handle(DefaultRequest request, CancellationToken cancellationToken)
        {
            // 處理邏輯
            return Task.FromResult("Handled DefaultRequest!");
        }
    }
}
