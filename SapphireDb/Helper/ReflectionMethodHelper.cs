using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace SapphireDb.Helper
{
    static class ReflectionMethodHelper
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> HandlerHandleMethods =
            new ConcurrentDictionary<Type, MethodInfo>();
        
        public static MethodInfo GetHandlerHandleMethod(this Type handlerType)
        {
            if (HandlerHandleMethods.TryGetValue(handlerType, out MethodInfo methodInfo))
            {
                return methodInfo;
            }

            methodInfo = handlerType.GetMethod("Handle");
            HandlerHandleMethods.TryAdd(handlerType, methodInfo);
            
            return methodInfo;
        }
        
        private static readonly MethodInfo QueryableWhere = typeof(Queryable)
            .GetMethods(BindingFlags.Static|BindingFlags.Public).FirstOrDefault(mi => mi.Name == "Where");

        public static MethodInfo GetGenericWhere(Type parameterType)
        {
            return QueryableWhere.MakeGenericMethod(parameterType);
        }
        
        public static readonly MethodInfo StringToLower = typeof(string).GetMethod(nameof(string.ToLower), new Type[] { });
        public static readonly MethodInfo StringContains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        public static readonly MethodInfo StringStartsWith = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
        public static readonly MethodInfo StringEndsWith = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
    }
}