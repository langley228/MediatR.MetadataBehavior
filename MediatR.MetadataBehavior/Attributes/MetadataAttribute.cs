using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MetadataBehavior.Attributes
{
    /// <summary>
    /// 用於定義 Mediatr Request 的自訂行為 (Behavior)。
    /// 此屬性可與 <see cref="DefaultMetadataAttribute"/> 和 <see cref="RequestAttribute"/> 搭配使用，
    /// 用於指定行為的類型、執行順序、生命週期等設定。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MetadataAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="MetadataAttribute"/> 類別的新實例。
        /// </summary>
        /// <param name="behaviorGenericType">
        /// 行為的泛型類型：
        /// - typeof(ClassName)：非泛型類型。
        /// - typeof(ClassName&lt;&gt;)：泛型類型，參數為 Request 或其他型別。
        /// - typeof(ClassName&lt;,&gt;)：泛型類型，參數為 Request、Response 或其他型別。
        /// - typeof(ClassName&lt;,,&gt;)：泛型類型，參數為 Request、Response、其他型別。
        /// 注意：Request 必須繼承 <see cref="IRequest"/> 或 <see cref="IRequest{TResponse}"/>，
        /// 否則所有泛型參數將被視為其他型別。
        /// </param>
        /// <param name="isDefault">是否為默認行為。</param>
        /// <param name="serviceLifetime">服務的生命週期，默認為 Scoped。</param>
        /// <param name="order">行為的執行順序，數值越小優先級越高，默認為 0。</param>
        public MetadataAttribute(
            Type behaviorGenericType,
            bool isDefault = false,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0)
        {
            BehaviorGenericType = behaviorGenericType;
            ServiceLifetime = serviceLifetime;
            Order = order;
            IsDefault = isDefault;
        }

        /// <summary>
        /// 行為的泛型類型。
        /// </summary>
        public Type BehaviorGenericType { get; }

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
        /// 是否為默認行為。
        /// </summary>
        public bool IsDefault { get; }
    }
}
