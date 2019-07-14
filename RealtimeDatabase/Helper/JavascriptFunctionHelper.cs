using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace RealtimeDatabase.Helper
{
    static class JavascriptFunctionHelper
    {
        public static Function CreateFunction(this string functionString, object[] contextData)
        {
            Context context = new Context();

            context.Eval("var __functionParsed__ = " + functionString + ";");
            context.Eval("var __contextData__ = JSON.parse('" + JsonHelper.Serialize(contextData) + "');");
            context.Eval("var __functionWrapper__ = function (dataObjectString) { var dataObject = JSON.parse(dataObjectString);" +
                         "return __functionParsed__(dataObject, __contextData__); };");

            return context.GetVariable("__functionWrapper__").As<Function>();
        }
    }
}
