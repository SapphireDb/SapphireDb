using System;
using System.Collections.Generic;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jurassic;
using React;
using React.TinyIoC;

namespace RealtimeDatabase.Helper
{
    static class JavascriptFunctionHelper
    {
        static JavascriptFunctionHelper()
        {

        }

        private static string CompileJs(string jsInput)
        {
            IBabel babel = ReactEnvironment.Current.Babel;
            return babel.Transform(jsInput);
        }

        private static Func<object, T> CreateFunction<T>(string functionString, object[] contextData)
        {
            IJsEngine engine = JsEngineSwitcher.Current.CreateDefaultEngine();

            string rawFunctionString = $"var __rawFunction__ = {functionString};";
            string contextDataString = $"var __contextData__ = JSON.parse('{JsonHelper.Serialize(contextData)}');";
            string functionWrapperString = $"function __functionWrapper__(dataObjectString) {{" +
                                     $"var dataObject = JSON.parse(dataObjectString);" +
                                     $"return __rawFunction__(dataObject, __contextData__);" +
                                     $"}};";

            string rawJs = $"{rawFunctionString}{contextDataString}{functionWrapperString}";
            string compiledJs = CompileJs(rawJs);

            engine.Evaluate(compiledJs);

            return (dataObject) =>
            {
                string dataObjectString = JsonHelper.Serialize(dataObject);
                return engine.CallFunction<T>("__functionWrapper__", dataObjectString);
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
