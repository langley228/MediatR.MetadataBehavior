using System;
using System.Runtime.CompilerServices;
using MediatR.PipelineExtensions.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.PipelineExtensions.Attributes
{
    /// <summary>
    /// 設置使用 Metadata Behavior 方式 的 Mediatr Request, 
    /// <see cref="MetadataAttribute"/>、<see cref="DefaultMetadataAttribute"/>、<see cref="RequestAttribute"/>搭配使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequestAttribute :
        Attribute
    {
        /// <summary>
        /// 使用 Request 設置 Metadata 為 Default 的流程
        /// </summary>
        /// <param name="serviceLifetime"></param>
        /// <param name="order"></param>
        /// <param name="canUseDefault"></param>
        public RequestAttribute(
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0) : this(
                behaviorFactoryGenericType: typeof(DefaultMetadataBehaviorFactory<,>),
                metadataType: null,
                serviceLifetime: serviceLifetime,
                order: order,
                canUseDefault: true)
        {
        }

        /// <summary>
        /// 使用 Request 設置 Metadata 為 Default 的流程
        /// </summary>
        /// <param name="metadataType"></param>
        /// <param name="serviceLifetime"></param>
        /// <param name="order"></param>
        public RequestAttribute(
            Type metadataType,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0) : this(
                behaviorFactoryGenericType: typeof(DefaultMetadataBehaviorFactory<,>),
                metadataType: metadataType,
                serviceLifetime: serviceLifetime,
                order: order,
                canUseDefault: true)
        {
        }

        /// <summary>
        /// 使用工廠所設置的自訂流程 或 Request 設置 Metadata 為 Default 的流程
        /// </summary>
        /// <param name="behaviorFactoryGenericType"></param>
        /// <param name="serviceLifetime"></param>
        /// <param name="order"></param>
        public RequestAttribute(
            Type behaviorFactoryGenericType,
            bool canUseDefault,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0) : this(
                behaviorFactoryGenericType: behaviorFactoryGenericType,
                metadataType: null,
                serviceLifetime: serviceLifetime,
                order: order,
                canUseDefault: canUseDefault)
        {
        }

        /// <summary>
        /// 使用工廠所設置的自訂流程 或 metadataType 設置 Metadata 為 Default 的流程
        /// </summary>
        /// <param name="behaviorFactoryGenericType"></param>
        /// <param name="metadataType"></param>
        /// <param name="canUseDefault"></param>
        /// <param name="serviceLifetime"></param>
        /// <param name="order"></param>
        public RequestAttribute(
            Type behaviorFactoryGenericType,
            Type metadataType,
            bool canUseDefault,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0)
        {
            BehaviorFactoryGenericType = behaviorFactoryGenericType;
            MetadataType = metadataType;
            ServiceLifetime = serviceLifetime;
            Order = order;
            CanUseDefault = canUseDefault;
        }

        public Type BehaviorFactoryGenericType { get; }

        /// <summary>
        /// Request, Response 以外的其他型別
        /// </summary>
        public Type[] GenericTypeParameters { get; set; }

        public ServiceLifetime ServiceLifetime { get; }

        public int Order { get; }

        public bool CanUseDefault { get; }

        public Type MetadataType { get; }
    }
}
