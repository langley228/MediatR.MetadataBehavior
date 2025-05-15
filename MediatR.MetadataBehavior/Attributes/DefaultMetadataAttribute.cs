using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MetadataBehavior.Attributes
{
    /// <summary>
    /// 用於定義 Mediatr Request 的預設行為 (Default Behavior)。
    /// 此屬性可與 <see cref="MetadataAttribute"/> 和 <see cref="RequestAttribute"/> 搭配使用，
    /// 用於指定預設的行為類型、執行順序和生命週期。
    /// </summary>
    public class DefaultMetadataAttribute : MetadataAttribute
    {
        /// <summary>
        /// 初始化 <see cref="DefaultMetadataAttribute"/> 類別的新實例。
        /// </summary>
        /// <param name="behaviorGenericType">
        /// 行為的泛型類型：
        /// - typeof(ClassName)：非泛型類型。
        /// - typeof(ClassName&lt;&gt;)：泛型類型，參數為 Request 或其他型別。
        /// - typeof(ClassName&lt;,&gt;)：泛型類型，參數為 Request、Response 或其他型別。
        /// - typeof(ClassName&lt;,,&gt;)：泛型類型，參數為 Request、Response、其他型別。
        /// </param>
        /// <param name="serviceLifetime">服務的生命週期，默認為 Scoped。</param>
        /// <param name="order">行為的執行順序，數值越小優先級越高，默認為 0。</param>
        public DefaultMetadataAttribute(
            Type behaviorGenericType,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            [CallerLineNumber] int order = 0) 
            : base(behaviorGenericType, true, serviceLifetime, order)
        {
        }
    }
}
