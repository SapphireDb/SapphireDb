using System;
using System.Collections.Generic;
using ChakraCore.NET;
using Newtonsoft.Json;

namespace RealtimeDatabase.Helper
{
    static class JavascriptFunctionHelper
    {
        private static Func<object, T> CreateFunction<T>(string functionString, object[] contextData)
        {
            ChakraRuntime runtime = ChakraRuntime.Create();
            ChakraContext context = runtime.CreateContext(false);
            context.RunScript($"let __rawFunction__ = {functionString};");
            context.RunScript($"let __contextData__ = JSON.parse('{JsonHelper.Serialize(contextData)}');");
            string functionWrapper = $"function __functionWrapper__(dataObjectString) {{" +
                                     $"let dataObject = JSON.parse(dataObjectString);" +
                                     $"return __rawFunction__(dataObject, __contextData__);" +
                                     $"}};";
            context.RunScript(functionWrapper);

            return (dataObject) =>
            {
                string dataObjectString = JsonHelper.Serialize(dataObject);
                return context.GlobalObject.CallFunction<string, T>("__functionWrapper__", dataObjectString);
            };
        }

        public static Func<object, bool> CreateBoolFunction(this string functionString, object[] contextData)
        {
            return CreateFunction<bool>(functionString, contextData);
        }

        public static Func<object, string> CreatePredicateFunction(this string functionString, object[] contextData)
        {
            return CreateFunction<string>(functionString, contextData);
        }
    }
}
