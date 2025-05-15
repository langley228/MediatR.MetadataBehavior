using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR.MetadataBehavior.Attributes;
using MediatR.MetadataBehavior.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.MetadataBehavior.Extensions
{
    /// <summary>
    /// 提供 IServiceCollection 的擴展方法，用於註冊 MediatR Metadata 行為。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 註冊指定程序集中的 MediatR Metadata 行為。
        /// </summary>
        /// <param name="services">IServiceCollection 實例。</param>
        /// <param name="assembly">包含行為定義的程序集。</param>
        /// <returns>更新後的 IServiceCollection 實例。</returns>
        public static IServiceCollection AddMediatRMetadataBehavior(this IServiceCollection services, Assembly assembly)
            => services.AddMediatRMetadataBehavior(new[] { assembly });

        /// <summary>
        /// 註冊多個程序集中的 MediatR Metadata 行為。
        /// </summary>
        /// <param name="services">IServiceCollection 實例。</param>
        /// <param name="assemblies">包含行為定義的程序集集合。</param>
        /// <returns>更新後的 IServiceCollection 實例。</returns>
        public static IServiceCollection AddMediatRMetadataBehavior(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var requestsWithAttributes = assemblies
                .Distinct()
                .SelectMany(a => a.DefinedTypes)
                .Where(x =>
                        x.IsMediatRequest() &&
                        x.IsDefinedBehaviorAttribute());

            foreach (var request in requestsWithAttributes)
            {
                var attributes = request.GetBehaviorRequestAttributes();
                // 每個 request 只能設定一個 BehaviorRequestAttribute，若多個則拋出錯誤
                var metadataFactoryAttr = attributes.SingleOrDefault(att => att.IsAssignableToBehaviorRequest<RequestAttribute>());
                services.AddMetadataBehaviorService(
                    requestInfo: request,
                    metadataFactoryAttr: metadataFactoryAttr);
            }

            return services;
        }

        /// <summary>
        /// 確認行為順序是否為指定的泛型型別。
        /// </summary>
        /// <typeparam name="T">目標泛型型別。</typeparam>
        /// <param name="behaviorOrder">行為順序屬性。</param>
        /// <returns>是否為指定泛型型別。</returns>
        private static bool IsAssignableToBehaviorRequest<T>(
            this RequestAttribute behaviorOrder)
            where T : RequestAttribute
        {
            return behaviorOrder.GetType().IsAssignableTo(typeof(T));
        }

        /// <summary>
        /// 取得指定型別的行為屬性集合。
        /// </summary>
        /// <param name="type">目標型別。</param>
        /// <returns>行為屬性集合。</returns>
        private static IEnumerable<RequestAttribute>
            GetBehaviorRequestAttributes(this Type type)
        {
            return type.GetCustomAttributes(false)
                .Where(att => att.GetType().IsAssignableTo(typeof(RequestAttribute)))
                .Select(att => att as RequestAttribute)
                .OrderBy(x => x.Order);
        }

        /// <summary>
        /// 註冊 Metadata 行為服務。
        /// </summary>
        /// <param name="services">IServiceCollection 實例，用於註冊服務。</param>
        /// <param name="requestInfo">請求型別資訊，表示需要處理的 MediatR Request 型別。</param>
        /// <param name="metadataFactoryAttr">Metadata 工廠屬性，包含行為工廠的相關設定。</param>
        private static void AddMetadataBehaviorService(
            this IServiceCollection services,
            TypeInfo requestInfo,
            RequestAttribute metadataFactoryAttr)
        {
            // 取得 Request 和 Response 型別
            (Type request, Type response) = requestInfo.GetRequestAndResponseType();

            // 註冊行為工廠作為 IPipelineBehavior
            services.AddBehaviorService(
                genericInterfaceType: typeof(IPipelineBehavior<,>), // 泛型介面型別
                attBehaviorType: metadataFactoryAttr.BehaviorFactoryGenericType, // 行為工廠型別
                request: request, // Request 型別
                response: response, // Response 型別
                genericTypeParameters: metadataFactoryAttr.GenericTypeParameters, // 泛型參數
                serviceLifetime: metadataFactoryAttr.ServiceLifetime); // 服務生命週期

            // 確定 Metadata 型別，若未指定則使用 Request 型別
            var metadataType = metadataFactoryAttr.MetadataType;
            if (metadataType == null)
                metadataType = request;
            if (metadataType == null)
                return;

            // 取得 Metadata 型別上的所有 MetadataAttribute
            var attributes = metadataType
                    .GetCustomAttributes(false)
                    .Where(att => att.GetType().IsAssignableTo(typeof(MetadataAttribute))) // 過濾出 MetadataAttribute
                    .Select(att => att as MetadataAttribute) // 轉換為 MetadataAttribute
                    .OrderBy(x => x.Order); // 按 Order 屬性排序

            // 為每個 MetadataAttribute 註冊對應的行為服務
            foreach (var attribute in attributes)
            {
                services.AddBehaviorService(
                    genericInterfaceType: typeof(IMetadataBehavior<,>), // 泛型介面型別
                    attBehaviorType: attribute.BehaviorGenericType, // 行為型別
                    request: request, // Request 型別
                    response: response, // Response 型別
                    genericTypeParameters: attribute.GenericTypeParameters, // 泛型參數
                    serviceLifetime: attribute.ServiceLifetime); // 服務生命週期
            }
        }

        /// <summary>
        /// 註冊行為服務。
        /// </summary>
        /// <param name="services">IServiceCollection 實例。</param>
        /// <param name="genericInterfaceType">泛型介面型別。</param>
        /// <param name="attBehaviorType">行為型別。</param>
        /// <param name="request">請求型別。</param>
        /// <param name="response">回應型別。</param>
        /// <param name="genericTypeParameters">泛型參數。</param>
        /// <param name="serviceLifetime">服務生命週期。</param>
        private static void AddBehaviorService(
            this IServiceCollection services,
            Type genericInterfaceType,
            Type attBehaviorType,
            Type request,
            Type response,
            Type[] genericTypeParameters,
            ServiceLifetime serviceLifetime)
        {
            var behaviorType = attBehaviorType.MakeBehaviorType(
                genericInterfaceType: genericInterfaceType,
                request: request,
                response: response,
                genericTypeParameters: genericTypeParameters);

            services.AddBehaviorService(
                        behaviorType: behaviorType,
                        interfaceType: genericInterfaceType.MakeGenericType(request, response),
                        serviceLifetime: serviceLifetime);
        }

        /// <summary>
        /// 註冊行為服務。
        /// </summary>
        /// <param name="services">IServiceCollection 實例。</param>
        /// <param name="behaviorType">行為型別。</param>
        /// <param name="interfaceType">介面型別。</param>
        /// <param name="serviceLifetime">服務生命週期。</param>
        private static void AddBehaviorService(
            this IServiceCollection services,
            Type behaviorType,
            Type interfaceType,
            ServiceLifetime serviceLifetime)
        {
            services.Add(new ServiceDescriptor(
                interfaceType,
                behaviorType,
                serviceLifetime));
        }

        /// <summary>
        /// 確認是否定義了 BehaviorAttribute。
        /// </summary>
        /// <param name="typeInfo">目標型別資訊。</param>
        /// <returns>是否定義了 BehaviorAttribute。</returns>
        private static bool IsDefinedBehaviorAttribute(this TypeInfo typeInfo)
        {
            return Attribute.IsDefined(typeInfo, typeof(RequestAttribute));
        }
    }
}
