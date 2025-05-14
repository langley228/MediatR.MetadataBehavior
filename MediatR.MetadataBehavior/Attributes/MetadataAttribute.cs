using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.PipelineExtensions.Attributes
{
    /// <summary>
    /// Mediatr Request 可使用自訂 Behavior, 
    /// <see cref="MetadataAttribute"/>、<see cref="DefaultMetadataAttribute"/>、<see cref="RequestAttribute"/>搭配使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MetadataAttribute :
        Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="behaviorGenericType">
        /// typeof(ClassName) : 實體型別非泛型
        /// typeof(ClassName<>) : 實體型別泛型參數 Request 或 其他型別
        /// typeof(ClassName<,>) : 實體型別泛型參數 Request、Response 或 Request、其他型別 或 其他型別
        /// typeof(ClassName<,,>) : 實體型別泛型參數 Request、Response、其他型別 或 Request、其他型別 或 其他型別
        /// 注意: Request 要記得繼承 IRequest 或 IRequest<Response>, 否則視為所有泛型參數都使用其他型別</param>
        /// <param name="serviceLifetime"></param>
        /// <param name="isDefault"></param>
        /// <param name="order"></param>
        public MetadataAttribute(
            Type behaviorGenericType,
            bool isDefault = false,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0)
        {
            BehaviorGenericType = behaviorGenericType;
            ServiceLifetime = serviceLifetime;
            ServiceLifetime = serviceLifetime;
            Order = order;
            IsDefault = isDefault;
        }

        public Type BehaviorGenericType { get; }

        /// <summary>
        /// Request, Response 以外的其他型別
        /// </summary>
        public Type[] GenericTypeParameters { get; set; }

        public ServiceLifetime ServiceLifetime { get; }

        public int Order { get; }

        public bool IsDefault { get; }
    }
}
