using System;
using System.Collections.Generic;
using ChakraCore.NET;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Newtonsoft.Json;
using V8.Net;

namespace RealtimeDatabase.Helper
{
    static class JavascriptFunctionHelper
    {
        private static dynamic CreateFunction(string functionString, object[] contextData)
        {
            //ChakraRuntime runtime = ChakraRuntime.Create();
            //ChakraContext context = runtime.CreateContext(false);
            //context.RunScript($"let __rawFunction__ = {functionString};");
            //context.RunScript($"let __contextData__ = JSON.parse('{JsonHelper.Serialize(contextData)}');");
            //string functionWrapper = $"function __functionWrapper__(dataObjectString) {{" +
            //                         $"let dataObject = JSON.parse(dataObjectString);" +
            //                         $"return __rawFunction__(dataObject, __contextData__);" +
            //                         $"}};";
            //context.RunScript(functionWrapper);

            //return (dataObject) =>
            //{
            //    string dataObjectString = JsonHelper.Serialize(dataObject);
            //    return context.GlobalObject.CallFunction<string, T>("__functionWrapper__", dataObjectString);
            //};

            //Engine engine = new Engine();
            //engine.Execute($"var __rawFunction__ = {functionString};");
            //engine.Execute($"var __contextData__ = JSON.parse('{JsonHelper.Serialize(contextData)}');");
            //string functionWrapperString = $"function __functionWrapper__(dataObjectString) {{" +
            //                         $"var dataObject = JSON.parse(dataObjectString);" +
            //                         $"return __rawFunction__(dataObject, __contextData__);" +
            //                         $"}};";
            //engine.Execute(functionWrapperString);
            //return engine.GetValue("__functionWrapper__");

            V8Engine engine = new V8Engine();
            engine.Execute($"var __rawFunction__ = {functionString};");
            engine.Execute($"var __contextData__ = JSON.parse('{JsonHelper.Serialize(contextData)}');");
            string functionWrapperString = $"function __functionWrapper__(dataObjectString) {{" +
                                     $"var dataObject = JSON.parse(dataObjectString);" +
                                     $"return __rawFunction__(dataObject, __contextData__);" +
                                     $"}};";
            engine.Execute(functionWrapperString);
            Handle
            return engine.DynamicGlobalObject.__functionWrapper__;
            engine.GlobalObject.GetProperty("__functionWrapper__");
            //return engine.GetValue("__functionWrapper__");
        }

        public static Func<object, bool> CreateBoolFunction(this string functionString, object[] contextData)
        {
            dynamic functionWrapper = CreateFunction(functionString, contextData);

            return (dataObject) =>
            {
                string dataObjectString = JsonHelper.Serialize(dataObject);
                return functionWrapper(dataObjectString).ToString() == "true";
            };
        }

        public static Func<object, string> CreatePredicateFunction(this string functionString, object[] contextData)
        {
            dynamic functionWrapper = CreateFunction(functionString, contextData);

            return (dataObject) =>
            {
                string dataObjectString = JsonHelper.Serialize(dataObject);
                Handle result = functionWrapper(dataObjectString);
                return result.ToString();
            };
        }
    }
}
