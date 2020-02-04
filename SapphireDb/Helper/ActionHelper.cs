using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class ActionHelper
    {
        private static readonly ConcurrentDictionary<Type, ActionHandlerAttributesInfo> ActionHandlerAttributesInfos = new ConcurrentDictionary<Type, ActionHandlerAttributesInfo>();

        public static ActionHandlerAttributesInfo GetActionHandlerAttributesInfo(this Type actionHandlerType)
        {
            if (ActionHandlerAttributesInfos.TryGetValue(actionHandlerType, out ActionHandlerAttributesInfo authActionHandlerInfo))
            {
                return authActionHandlerInfo;
            }

            authActionHandlerInfo = new ActionHandlerAttributesInfo(actionHandlerType);
            ActionHandlerAttributesInfos.TryAdd(actionHandlerType, authActionHandlerInfo);

            return authActionHandlerInfo;
        }
        
        private static readonly ConcurrentDictionary<MethodInfo, ActionAttributesInfo> ActionAttributesInfos = new ConcurrentDictionary<MethodInfo, ActionAttributesInfo>();

        public static ActionAttributesInfo GetActionAttributesInfo(this MethodInfo action)
        {
            if (ActionAttributesInfos.TryGetValue(action, out ActionAttributesInfo authActionInfo))
            {
                return authActionInfo;
            }

            authActionInfo = new ActionAttributesInfo(action);
            ActionAttributesInfos.TryAdd(action, authActionInfo);

            return authActionInfo;
        }

        public static bool HandleAsyncEnumerable(object result, Action<object> handleResult)
        {
            Type resultType = result.GetType();
            Type asyncEnumerableType = resultType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));

            if (asyncEnumerableType == null)
            {
                return false;
            }
            
            Type valueType = asyncEnumerableType.GetGenericArguments().FirstOrDefault();

            typeof(ActionHelper)
                .GetMethod(nameof(CallHandleEnumerable), BindingFlags.Static|BindingFlags.NonPublic)?
                .MakeGenericMethod(asyncEnumerableType, valueType)
                .Invoke(null, new[] {result, handleResult});
            
            return true;
        }

        public static async Task<object> HandleAsyncResult(object result)
        {
            Type resultType = result.GetType();
            Type asyncResultType = resultType.GetInterfaces()
                .FirstOrDefault(i => i == typeof(IAsyncResult));

            if (asyncResultType != null)
            {
                return await (dynamic) result;
            }

            return result;
        }
        
        private static void CallHandleEnumerable<T, TValue>(object result, Action<object> handleResult) where T : IAsyncEnumerable<TValue>
        {
            HandleEnumerable<T, TValue>(result, handleResult).Wait();
        }

        private static async Task HandleEnumerable<T, TValue>(object result, Action<object> handleResult) where T : IAsyncEnumerable<TValue>
        {
            await foreach (TValue value in (T) result)
            {
                handleResult(value);
            }
        }
    }
}