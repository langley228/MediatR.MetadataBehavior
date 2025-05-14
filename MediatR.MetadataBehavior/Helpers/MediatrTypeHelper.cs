using System;
using System.Linq;
using System.Reflection;
using MediatR.PipelineExtensions.Models;

namespace MediatR.PipelineExtensions.Helpers
{
    public static class MediatrTypeHelper
    {
        /// <summary>
        /// 取得 MediatR Request、Response 型別
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        internal static (Type request, Type response)
            GetRequestAndResponseType(this TypeInfo requestInfo)
        {
            return (requestInfo.AsType(), requestInfo.GetResponseType());
        }

        /// <summary>
        /// 取得 MediatR Request 的 Response 型別
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        internal static Type GetResponseType(this TypeInfo requestInfo)
        {
            Type respType = typeof(Unit);
            if (requestInfo.IsAssignableToGenericType(typeof(IRequest<>)))
            {
                var t = requestInfo.GetInterfaces().FirstOrDefault(i => i.IsAssignableToGenericType(typeof(IRequest<>)));
                respType = t.GenericTypeArguments[0];
            }
            return respType;
        }

        /// <summary>
        /// 是否為 MediatR Request
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        internal static bool IsMediatRequest(this TypeInfo typeInfo)
        {
            return typeInfo.ImplementedInterfaces.Contains(typeof(IRequest))
                || typeInfo.IsAssignableToGenericType(typeof(IRequest<>));
        }

        internal static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            if (givenType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            var baseType = givenType.BaseType;
            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }

        /// <summary>
        /// 取得 Behavior 實體型別
        /// </summary>
        /// <param name="attBehaviorType"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static Type
            MakePipelineBehaviorType(
            this Type attBehaviorType,
            Type request,
            Type response,
            Type[] genericTypeParameters)
        {
            return MakeBehaviorType(
                attBehaviorType: attBehaviorType,
                genericInterfaceType: typeof(IPipelineBehavior<,>),
                request: request,
                response: response,
                genericTypeParameters: genericTypeParameters);
        }

        internal static Type
            MakeBehaviorType(
                this Type attBehaviorType,
                Type genericInterfaceType,
                Type request,
                Type response,
                Type[] genericTypeParameters
            )
        {
            Type behaviorType = null;
            if (attBehaviorType.IsAssignableToGenericType(genericInterfaceType))
            {
                TypeInfo behaviorTypeInfo = attBehaviorType.GetTypeInfo();
                behaviorType = behaviorTypeInfo;
                if (behaviorTypeInfo.GenericTypeParameters.Length > 0)
                {
                    Type[] typeArguments = GetMakeGenericTypeArguments(request, response, genericTypeParameters, behaviorTypeInfo);
                    behaviorType = behaviorType.MakeGenericType(typeArguments);
                }
                else if (genericTypeParameters != null && genericTypeParameters.Length > 0)
                    throw new ArgumentException("genericTypeParameters not match"); //參數數量不對應
            }

            if (behaviorType == null)
                throw new InvalidOperationException($"attBehaviorType({attBehaviorType}) not match genericInterfaceType({genericInterfaceType})"); //參數數量不對應
            return behaviorType;
        }

        /// <summary>
        /// 取得 Behavior 實體型別
        /// </summary>
        /// <param name="attBehaviorType"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static Type
            MakeMetadataBehaviorType(
                this Type attBehaviorType,
                Type request,
                Type response,
                Type[] genericTypeParameters)
        {
            return MakeBehaviorType(
                attBehaviorType: attBehaviorType,
                genericInterfaceType: typeof(IMetadataBehavior<,>),
                request: request,
                response: response,
                genericTypeParameters: genericTypeParameters);
        }

        /// <summary>
        /// 取得 MakeGenericType 所需要的型別參數
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="genericTypeParameters"></param>
        /// <param name="behaviorTypeInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Type[] GetMakeGenericTypeArguments(
            Type request,
            Type response,
            Type[] genericTypeParameters,
            TypeInfo behaviorTypeInfo)
        {
            Type[] typeArguments = behaviorTypeInfo.GenericTypeParameters.Select(s => null as Type).ToArray();
            //尋找 request 型別位置
            var reqIndex = Array.FindIndex(behaviorTypeInfo.GenericTypeParameters, t => t.GetTypeInfo().IsMediatRequest());
            var respIndex = -1;
            if (reqIndex > -1)
            {
                //尋找 response 型別位置
                var reqParameter = behaviorTypeInfo.GenericTypeParameters[reqIndex];
                var reqInterface = reqParameter.GetInterfaces().FirstOrDefault(t => t.GetTypeInfo().IsMediatRequest());
                if (reqInterface.GenericTypeArguments.Length > 0)
                    respIndex = Array.FindIndex(behaviorTypeInfo.GenericTypeParameters, t => t == reqInterface.GenericTypeArguments[0]);
            }
            //填入 request 型別
            if (reqIndex > -1)
                typeArguments[reqIndex] = request;
            //填入 response 型別
            if (respIndex > -1)
                typeArguments[respIndex] = response;

            //依序填入剩下的型別
            if (genericTypeParameters != null && genericTypeParameters.Length > 0)
            {
                foreach (var param in genericTypeParameters)
                {
                    var index = Array.FindIndex(typeArguments, t => t == null);
                    if (index > -1)
                        typeArguments[index] = param;
                    else
                        throw new ArgumentException("genericTypeParameters not match"); //參數位置不夠
                }
            }
            //所有型別參數位置都要填到
            if (Array.FindIndex(typeArguments, t => t == null) > -1)
                throw new ArgumentException("genericTypeParameters not match");
            return typeArguments;
        }
    }
}
