using System;
using System.Runtime.CompilerServices;
using MediatR.MetadataBehavior.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MetadataBehavior.Attributes
{
    /// <summary>
    /// 設置使用 Metadata Behavior 的 Mediatr Request。
    /// 此屬性用於標記請求類型，並定義其行為工廠、Metadata 類型和其他相關設定。
    /// 搭配 <see cref="MetadataAttribute"/> 和 <see cref="DefaultMetadataAttribute"/> 使用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequestAttribute : Attribute
    {
        /// <summary>
        /// 使用 Request 設置 Metadata 為默認流程。
        /// </summary>
        /// <param name="serviceLifetime">服務的生命週期，默認為 Scoped。</param>
        /// <param name="order">行為的執行順序，默認為 0。</param>
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
        /// 使用 Request 設置 Metadata 為默認流程，並指定 Metadata 類型。
        /// </summary>
        /// <param name="metadataType">Metadata 類型，用於定義行為的額外資訊。</param>
        /// <param name="serviceLifetime">服務的生命週期，默認為 Scoped。</param>
        /// <param name="order">行為的執行順序，默認為 0。</param>
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
        /// 使用工廠設置自訂流程，或使用 Request 設置 Metadata 為默認流程。
        /// </summary>
        /// <param name="behaviorFactoryGenericType">行為工廠的泛型類型。</param>
        /// <param name="canUseDefault">是否允許使用默認行為。</param>
        /// <param name="serviceLifetime">服務的生命週期，默認為 Scoped。</param>
        /// <param name="order">行為的執行順序，默認為 0。</param>
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
        /// 使用工廠設置自訂流程，或使用指定的 Metadata 類型設置默認流程。
        /// </summary>
        /// <param name="behaviorFactoryGenericType">行為工廠的泛型類型。</param>
        /// <param name="metadataType">Metadata 類型，用於定義行為的額外資訊。</param>
        /// <param name="canUseDefault">是否允許使用默認行為。</param>
        /// <param name="serviceLifetime">服務的生命週期，默認為 Scoped。</param>
        /// <param name="order">行為的執行順序，默認為 0。</param>
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

        /// <summary>
        /// 行為工廠的泛型類型。
        /// </summary>
        public Type BehaviorFactoryGenericType { get; }

        /// <summary>
        /// Request 和 Response 以外的其他泛型參數。
        /// </summary>
        public Type[] GenericTypeParameters { get; set; }

        /// <summary>
        /// 服務的生命週期。
        /// </summary>
        public ServiceLifetime ServiceLifetime { get; }

        /// <summary>
        /// 行為的執行順序，數值越小優先級越高。
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// 是否允許使用默認行為。
        /// </summary>
        public bool CanUseDefault { get; }

        /// <summary>
        /// Metadata 類型，用於定義行為的額外資訊。
        /// </summary>
        public Type MetadataType { get; }
    }
}
