using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR.MetadataBehavior.Models;
using MediatR.MetadataBehavior.Sample.Models;

namespace MediatR.MetadataBehavior.Sample.Factories
{
    /// <summary>
    /// 自訂行為工廠，用於根據請求中的行為名稱篩選並執行對應的行為
    /// </summary>
    public class CustomBehaviorFactory<TRequest, TResponse> :
        MetadataBehaviorFactory<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// 建構函式，注入服務提供者
        /// </summary>
        /// <param name="serviceProvider">DI 容器的服務提供者</param>
        public CustomBehaviorFactory(
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// 映射行為，根據請求中的行為名稱篩選並執行對應的行為
        /// </summary>
        /// <param name="request">請求物件</param>
        /// <param name="behaviors">所有可用的行為集合</param>
        /// <param name="next">下一個處理函式</param>
        /// <param name="cancellationToken">取消標記</param>
        /// <returns>處理後的回應</returns>
        public override Task<TResponse> MapAndExecuteBehaviors(
            TRequest request,
            IEnumerable<IMetadataBehavior<TRequest, TResponse>> behaviors,
            Func<IEnumerable<IMetadataBehavior<TRequest, TResponse>>, CancellationToken, Task<TResponse>> next, 
            CancellationToken cancellationToken)
        {
            string[] behaviorNames = null;

            // 自訂流程：檢查請求是否為 CustomRequest，並提取行為名稱
            if (request is CustomRequest customRequest)
            {
                behaviorNames = customRequest.BehaviorNames;
            }

            // 根據行為名稱篩選對應的行為
            var findBehaviors = behaviorNames.Select(name => 
                behaviors.FirstOrDefault(b => GetBehaviorName(b.GetType()) == name));

            // 執行篩選後的行為集合
            return next(findBehaviors, cancellationToken);
        }

        /// <summary>
        /// 取得行為的名稱，處理泛型類型名稱的格式
        /// </summary>
        /// <param name="type">行為的類型</param>
        /// <returns>行為名稱</returns>
        private string GetBehaviorName(Type type)
        {
            // 如果是泛型類型，去除泛型參數部分
            if (type.IsGenericType)
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}";
            else
                return type.Name;
        }
    }
}
