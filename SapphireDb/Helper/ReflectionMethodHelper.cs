using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using SapphireDb.Models.Exceptions;

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
        
        public static MethodInfo GetMethodInfo(Type modelType, string methodName, Type returnType,
            BindingFlags bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }
            
            MethodInfo methodInfo = modelType.GetMethod(methodName, bindingFlags);

            if (methodInfo == null)
            {
                throw new MethodNotFoundException(modelType.Name, methodName);
            }
            
            if (!returnType.IsAssignableFrom(methodInfo.ReturnType))
            {
                throw new WrongReturnTypeException(modelType.Name, methodName, returnType.Name);
            }

            return methodInfo;
        }

        public static readonly MethodInfo StringToLower = typeof(string).GetMethod(nameof(string.ToLower), new Type[] { });
        public static readonly MethodInfo StringContains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        public static readonly MethodInfo StringStartsWith = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
        public static readonly MethodInfo StringEndsWith = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
    }
}