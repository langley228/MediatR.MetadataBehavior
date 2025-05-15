using System;
using System.Threading.Tasks;
using MediatR.MetadataBehavior.Sample.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MetadataBehavior.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 建立 DI 容器
            var services = new ServiceCollection();

            // 設定服務 (例如註冊 MediatR 和相關行為)
            new Startup().ConfigureServices(services);

            // 建立服務提供者
            var provider = services.BuildServiceProvider();

            // 取得 MediatR 實例，用於處理請求
            var mediator = provider.GetRequiredService<IMediator>();

            // 發送請求並處理不同的行為組合
            await ProcessDefaultRequest(mediator); // 無行為
            await ProcessCustomRequest(mediator, "LoggingBehavior"); // 僅 LoggingBehavior
            await ProcessCustomRequest(mediator, "ValidationBehavior"); // 僅 ValidationBehavior
            await ProcessCustomRequest(mediator, "LoggingBehavior", "ValidationBehavior"); // LoggingBehavior + ValidationBehavior
            await ProcessCustomRequest(mediator, "ValidationBehavior", "LoggingBehavior"); // ValidationBehavior + LoggingBehavior (順序不同)
        }

        /// <summary>
        /// 發送 SampleRequest 並輸出結果
        /// </summary>
        /// <param name="mediator">MediatR 實例</param>
        /// <param name="behaviorNamest">行為名稱陣列</param>
        private static async Task ProcessCustomRequest(
            IMediator mediator,
            params string[] behaviorNamest)
        {
            // 建立請求物件，包含指定的行為名稱
            var request = new CustomRequest
            {
                BehaviorNames = behaviorNamest
            };

            // 發送請求並取得回應
            var response = await mediator.Send(request);

            // 輸出請求與回應的資訊
            Console.WriteLine($"CustomRequest BehaviorNames:[{string.Join(",", request.BehaviorNames)}], Response: {response}\r\n");
        }

        private static async Task ProcessDefaultRequest(IMediator mediator)
        {
            // 建立請求物件，包含指定的行為名稱
            var request = new DefaultRequest();

            // 發送請求並取得回應
            var response = await mediator.Send(request);

            // 輸出請求與回應的資訊
            Console.WriteLine($"DefaultRequest Response: {response}\r\n");
        }
    }
}