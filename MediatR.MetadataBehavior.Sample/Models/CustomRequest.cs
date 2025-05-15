using MediatR.MetadataBehavior.Attributes;
using MediatR.MetadataBehavior.Sample.Factories;

namespace MediatR.MetadataBehavior.Sample.Models
{
    /// <summary>
    /// 範例請求類別，定義請求的行為與元數據
    /// </summary>
    [Request(
        metadataType: typeof(RequestMetadata), // 指定元數據類型
        canUseDefault: true, // 是否允許使用預設行為
        behaviorFactoryGenericType: typeof(CustomBehaviorFactory<,>))] // 指定自訂行為工廠
    public class CustomRequest : IRequest<string>
    {
        /// <summary>
        /// 請求中包含的行為名稱，用於篩選執行的行為
        /// </summary>
        public string[] BehaviorNames { get; set; }
    }

}