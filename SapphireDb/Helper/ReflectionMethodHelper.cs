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
        
        public static MethodInfo GetMethodInfo(Type modelType, string methodName, Type returnType)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }
            
            MethodInfo methodInfo = modelType.GetMethod(methodName,
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (methodInfo == null || methodInfo.ReturnType != returnType)
            {
                throw new Exception("No suiting method was found");
            }

            return methodInfo;
        }
        
        public static readonly MethodInfo StringToLower = typeof(string).GetMethod(nameof(string.ToLower), new Type[] { });
        public static readonly MethodInfo StringContains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        public static readonly MethodInfo StringStartsWith = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
        public static readonly MethodInfo StringEndsWith = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
    }
}