using System;
using System.Linq;
using System.Reflection;
using MediatR.MetadataBehavior.Models;

namespace MediatR.MetadataBehavior.Extensions
{
    /// <summary>
    /// 提供與 MediatR 型別相關的擴展方法。
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 取得 MediatR Request 和 Response 型別。
        /// </summary>
        /// <param name="requestInfo">Request 的型別資訊。</param>
        /// <returns>包含 Request 和 Response 型別的元組。</returns>
        internal static (Type request, Type response)
            GetRequestAndResponseType(this TypeInfo requestInfo)
        {
            return (requestInfo.AsType(), requestInfo.GetResponseType());
        }

        /// <summary>
        /// 取得 MediatR Request 的 Response 型別。
        /// </summary>
        /// <param name="requestInfo">Request 的型別資訊。</param>
        /// <returns>Response 型別。</returns>
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
        /// 確認型別是否為 MediatR Request。
        /// </summary>
        /// <param name="typeInfo">型別資訊。</param>
        /// <returns>如果是 MediatR Request 則返回 true，否則返回 false。</returns>
        internal static bool IsMediatRequest(this TypeInfo typeInfo)
        {
            return typeInfo.ImplementedInterfaces.Contains(typeof(IRequest))
                || typeInfo.IsAssignableToGenericType(typeof(IRequest<>));
        }

        /// <summary>
        /// 確認型別是否可分配給指定的泛型型別。
        /// </summary>
        /// <param name="givenType">要檢查的型別。</param>
        /// <param name="genericType">目標泛型型別。</param>
        /// <returns>如果可分配則返回 true，否則返回 false。</returns>
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
        /// 取得 PipelineBehavior 的實體型別。
        /// </summary>
        /// <param name="attBehaviorType">行為型別。</param>
        /// <param name="request">Request 型別。</param>
        /// <param name="response">Response 型別。</param>
        /// <param name="genericTypeParameters">泛型參數。</param>
        /// <returns>PipelineBehavior 的實體型別。</returns>
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

        /// <summary>
        /// 取得行為的實體型別。
        /// </summary>
        /// <param name="attBehaviorType">行為型別，表示需要生成的行為類型。</param>
        /// <param name="genericInterfaceType">泛型介面型別，例如 IPipelineBehavior 或 IMetadataBehavior。</param>
        /// <param name="request">Request 型別，表示行為處理的請求型別。</param>
        /// <param name="response">Response 型別，表示行為處理的回應型別。</param>
        /// <param name="genericTypeParameters">泛型參數，用於補充行為型別的泛型定義。</param>
        /// <returns>行為的實體型別。</returns>
        /// <exception cref="ArgumentException">當泛型參數不匹配時拋出。</exception>
        /// <exception cref="InvalidOperationException">當行為型別與泛型介面型別不匹配時拋出。</exception>
        internal static Type MakeBehaviorType(
            this Type attBehaviorType,
            Type genericInterfaceType,
            Type request,
            Type response,
            Type[] genericTypeParameters
        )
        {
            Type behaviorType = null;

            // 確認行為型別是否可分配給指定的泛型介面型別
            if (attBehaviorType.IsAssignableToGenericType(genericInterfaceType))
            {
                // 取得行為型別的型別資訊
                TypeInfo behaviorTypeInfo = attBehaviorType.GetTypeInfo();
                behaviorType = behaviorTypeInfo;

                // 如果行為型別有泛型參數，則生成具體的泛型型別
                if (behaviorTypeInfo.GenericTypeParameters.Length > 0)
                {
                    // 取得泛型型別參數
                    Type[] typeArguments = GetMakeGenericTypeArguments(request, response, genericTypeParameters, behaviorTypeInfo);
                    // 使用泛型參數生成具體的行為型別
                    behaviorType = behaviorType.MakeGenericType(typeArguments);
                }
                // 如果行為型別沒有泛型參數，但提供了泛型參數，則拋出異常
                else if (genericTypeParameters != null && genericTypeParameters.Length > 0)
                {
                    throw new ArgumentException("genericTypeParameters not match"); // 泛型參數數量不對應
                }
            }

            // 如果行為型別無法分配給指定的泛型介面型別，則拋出異常
            if (behaviorType == null)
            {
                throw new InvalidOperationException($"attBehaviorType({attBehaviorType}) not match genericInterfaceType({genericInterfaceType})");
            }

            return behaviorType;
        }

        /// <summary>
        /// 取得 MetadataBehavior 的實體型別。
        /// </summary>
        /// <param name="attBehaviorType">行為型別。</param>
        /// <param name="request">Request 型別。</param>
        /// <param name="response">Response 型別。</param>
        /// <param name="genericTypeParameters">泛型參數。</param>
        /// <returns>MetadataBehavior 的實體型別。</returns>
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
        /// 取得 MakeGenericType 所需的型別參數。
        /// </summary>
        /// <param name="request">Request 型別。</param>
        /// <param name="response">Response 型別。</param>
        /// <param name="genericTypeParameters">泛型參數。</param>
        /// <param name="behaviorTypeInfo">行為型別資訊。</param>
        /// <returns>型別參數陣列。</returns>
        /// <exception cref="ArgumentException">當參數不匹配時拋出。</exception>
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
