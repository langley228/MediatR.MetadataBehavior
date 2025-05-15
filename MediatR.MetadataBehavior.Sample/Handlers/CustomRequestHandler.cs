using System.Threading;
using System.Threading.Tasks;
using MediatR.MetadataBehavior.Sample.Models;

namespace MediatR.MetadataBehavior.Sample.Handlers
{
    public class CustomRequestHandler : IRequestHandler<CustomRequest, string>
    {
        public Task<string> Handle(CustomRequest request, CancellationToken cancellationToken)
        {
            // 處理邏輯
            return Task.FromResult("Handled CustomRequest!");
        }
    }
}
