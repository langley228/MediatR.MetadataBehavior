using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR.PipelineExtensions.Attributes;
using MediatR.PipelineExtensions.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.PipelineExtensions.Helpers
{
    public static class ServiceCollectionHelper
    {
        public static IServiceCollection AddMediatRAttributedBehaviors(this IServiceCollection services, Assembly assembly)
            => services.AddMediatRAttributedBehaviors(new[] { assembly });

        public static IServiceCollection AddMediatRAttributedBehaviors(this IServiceCollection services, IEnumerable<Assembly> assemblies)
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
                //每個 request 只能 設定一個 BehaviorRequestAttribute, 多個就直接噴錯
                var metadataFactoryAttr = attributes.SingleOrDefault(att => att.IsAssignableToBehaviorRequest<RequestAttribute>());
                services.AddMetadataBehaviorService(
                    requestInfo: request,
                    metadataFactoryAttr: metadataFactoryAttr);
            }

            return services;
        }

        /// <summary>
        /// behaviorOrder 是否為 T 型別
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="behaviorOrder"></param>
        /// <returns></returns>
        private static bool IsAssignableToBehaviorRequest<T>(
            this RequestAttribute behaviorOrder)
            where T : RequestAttribute
        {
            return behaviorOrder.GetType().IsAssignableTo(typeof(T));
        }

        private static IEnumerable<RequestAttribute>
            GetBehaviorRequestAttributes(this Type type)
        {
            return type.GetCustomAttributes(false)
                .Where(att => att.GetType().IsAssignableTo(typeof(RequestAttribute)))
                .Select(att => att as RequestAttribute)
                .OrderBy(x => x.Order);
        }

        private static void AddMetadataBehaviorService(
            this IServiceCollection services,
            TypeInfo requestInfo,
            RequestAttribute metadataFactoryAttr)
        {
            (Type request, Type response) = requestInfo.GetRequestAndResponseType();
            //add behavior factory for IPipelineBehavior
            services.AddBehaviorService(
                genericInterfaceType: typeof(IPipelineBehavior<,>),
                attBehaviorType: metadataFactoryAttr.BehaviorFactoryGenericType,
                request: request,
                response: response,
                genericTypeParameters: metadataFactoryAttr.GenericTypeParameters,
                serviceLifetime: metadataFactoryAttr.ServiceLifetime);

            var metadataType = metadataFactoryAttr.MetadataType;
            if (metadataType == null)
                metadataType = request;
            if (metadataType == null)
                return;

            var attributes = metadataType
                    .GetCustomAttributes(false)
                    .Where(att => att.GetType().IsAssignableTo(typeof(MetadataAttribute)))
                    .Select(att => att as MetadataAttribute)
                    .OrderBy(x => x.Order);
            foreach (var attribute in attributes)
            {
                services.AddBehaviorService(
                    genericInterfaceType: typeof(IMetadataBehavior<,>),
                    attBehaviorType: attribute.BehaviorGenericType,
                    request: request,
                    response: response,
                    genericTypeParameters: attribute.GenericTypeParameters,
                    serviceLifetime: attribute.ServiceLifetime);
            }
        }

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
        /// Add Behavior Service
        /// </summary>
        /// <param name="services"></param>
        /// <param name="behaviorType"></param>
        /// <param name="interfaceType"></param>
        /// <param name="serviceLifetime"></param>
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
        /// 是否定義 BehaviorAttribute
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        private static bool IsDefinedBehaviorAttribute(this TypeInfo typeInfo)
        {
            return Attribute.IsDefined(typeInfo, typeof(RequestAttribute));
        }
    }
}
