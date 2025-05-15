using MediatR.MetadataBehavior.Attributes;

namespace MediatR.MetadataBehavior.Sample.Models
{
    /// <summary>
    /// 範例請求類別，定義請求的行為與元數據
    /// </summary>
    [Request(metadataType: typeof(RequestMetadata))]
    public class DefaultRequest : IRequest<string>
    {
    }
}
