using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.PipelineExtensions.Attributes
{
    /// <summary>
    /// Mediatr Request 可使用自訂 Behavior 且設為預設流程 , 
    /// <see cref="MetadataAttribute"/>、<see cref="DefaultMetadataAttribute"/>、<see cref="RequestAttribute"/>搭配使用
    /// </summary>
    public class DefaultMetadataAttribute : MetadataAttribute
    {
        public DefaultMetadataAttribute(
            Type behaviorGenericType,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0) : base(behaviorGenericType, true, serviceLifetime, order)
        {
        }
    }
}
