using System;
using System.Collections.Generic;
using JavaScriptEngineSwitcher.Core;

namespace RealtimeDatabase.Helper
{
    static class JavascriptFunctionHelper
    {
        private static Func<object, T> CreateFunction<T>(IJsEngine engine, string functionString, object[] contextData)
        {
            string rawFunctionString = $"var __rawFunction__ = {functionString};";
            string contextDataString = $"var __contextData__ = JSON.parse('{JsonHelper.Serialize(contextData)}');";
            string functionWrapperString = $"var __functionWrapper__ = (dataObjectString) => {{" +
                                     $"var dataObject = JSON.parse(dataObjectString);" +
                                     $"return __rawFunction__(dataObject, __contextData__);" +
                                     $"}};";

            string rawJs = $"{rawFunctionString}{contextDataString}{functionWrapperString}";

            engine.Execute(rawJs);

            return (dataObject) =>
            {
                string dataObjectString = JsonHelper.Serialize(dataObject);
                return engine.CallFunction<T>("__functionWrapper__", dataObjectString);
            };
        }

        public static Func<object, bool> CreateBoolFunction(this string functionString, object[] contextData, IJsEngine engine)
        {
            return CreateFunction<bool>(engine, functionString, contextData);
        }

        public static Func<object, string> CreatePredicateFunction(this string functionString, object[] contextData, IJsEngine engine)
        {
            return CreateFunction<string>(engine, functionString, contextData);
        }
    }
}
