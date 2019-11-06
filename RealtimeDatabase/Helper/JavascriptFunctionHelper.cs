using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;

namespace RealtimeDatabase.Helper
{
    static class JavascriptFunctionHelper
    {
        private static string WrapFunctionBody(string functionBody)
        {
            if (functionBody.StartsWith('{') && functionBody.EndsWith('}'))
            {
                return functionBody;
            }

            return $"{{ return {functionBody} }}";
        }

        private static string PrepareNewHeader(string functionHeader)
        {
            functionHeader = functionHeader.Replace("(", "").Replace(")", "");
            string[] headerParts = functionHeader.Split(',', 2).Select(v => v.Trim()).ToArray();

            if (headerParts.Length == 2)
            {
                string parametersTuple = headerParts[1];
                if (parametersTuple.StartsWith('[') && parametersTuple.EndsWith(']'))
                {
                    parametersTuple = parametersTuple.Replace("[", "").Replace("]", "");
                    string[] tupleFields = parametersTuple.Split(',').Select(v => v.Trim()).ToArray();
                    headerParts = new string[] { headerParts[0] }.Concat(tupleFields).ToArray();
                }
            }

            return $"function ({string.Join(',', headerParts)})";
        }

        private static string ConvertArrowFunction(string functionString)
        {
            if (!functionString.Contains("=>"))
            {
                return functionString;
            }

            string[] functionParts = functionString.Split("=>");

            string body = WrapFunctionBody(functionParts[1]);
            string header = PrepareNewHeader(functionParts[0]);

            return $"{header}{body}";
        }

        private static string PrepareContextDataParameter(object[] contextData)
        {
            if (!contextData.Any())
            {
                return "";
            }

            string[] contextDataStrings = contextData.Select(d =>
            {
                if (d is string stringValue)
                {
                    return $"'{stringValue}'";
                }

                return $"JSON.parse('{JsonHelper.Serialize(d)}')";
            }).ToArray();

            return "," + string.Join(',', contextDataStrings);
        }

        private static Func<object, T> CreateFunction<T>(IJsEngine engine, string functionString, object[] contextData)
        {
            string rawFunctionString = $"var __rawFunction__ = {ConvertArrowFunction(functionString)};";
            string functionWrapperString = $"function __functionWrapper__(dataObjectString) {{" +
                                     $"var dataObject = JSON.parse(dataObjectString);" +
                                     $"return __rawFunction__(dataObject{PrepareContextDataParameter(contextData)});" +
                                     $"}};";

            string rawJs = $"{rawFunctionString}{functionWrapperString}";

            engine.Execute(rawJs);

            return (dataObject) =>
            {
                string dataObjectString = JsonHelper.Serialize(dataObject);
                return engine.CallFunction<T>("__functionWrapper__", dataObjectString);
            };
        }

        // Version without wrapper for arrow functions and tuples
        //private static Func<object, T> CreateFunction<T>(IJsEngine engine, string functionString, object[] contextData)
        //{
        //    string rawFunctionString = $"var __rawFunction__ = {functionString};";
        //    string contextDataString = $"var __contextData__ = JSON.parse('{JsonHelper.Serialize(contextData)}');";
        //    string functionWrapperString = $"var __functionWrapper__ = (dataObjectString) => {{" +
        //                                   $"var dataObject = JSON.parse(dataObjectString);" +
        //                                   $"return __rawFunction__(dataObject, __contextData__);" +
        //                                   $"}};";

        //    string rawJs = $"{rawFunctionString}{contextDataString}{functionWrapperString}";

        //    engine.Execute(rawJs);

        //    return (dataObject) =>
        //    {
        //        string dataObjectString = JsonHelper.Serialize(dataObject);
        //        return engine.CallFunction<T>("__functionWrapper__", dataObjectString);
        //    };
        //}

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
