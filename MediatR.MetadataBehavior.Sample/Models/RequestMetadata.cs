using MediatR.MetadataBehavior.Attributes;
using MediatR.MetadataBehavior.Sample.Behaviors;    

namespace MediatR.MetadataBehavior.Sample.Models
{
    /// <summary>
    /// 定義範例請求的元數據類別，用於指定行為的預設與可選配置
    /// </summary>
    [DefaultMetadata(typeof(LoggingBehavior<,>))] // 指定預設行為為 LoggingBehavior
    [Metadata(typeof(ValidationBehavior<,>), isDefault: false)] // 指定非預設行為為 ValidationBehavior
    public class RequestMetadata
    {
        // 此類別僅用於元數據標註，無需其他成員
    }
}