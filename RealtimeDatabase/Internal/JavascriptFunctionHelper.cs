using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System;
using System.Collections.Generic;

namespace RealtimeDatabase.Internal
{
    static class JavascriptFunctionHelper
    {
        public static Function CreateFunction(this string functionString, Type attributeType, Dictionary<string, string> contextData)
        {
            Context context = new Context();

            if (contextData != null)
            {
                foreach (KeyValuePair<string, string> token in contextData)
                {
                    context.Eval("var " + token.Key + " = JSON.parse('" + token.Value + "');");
                }
            }

            context.Eval("var __functionParsed__ = " + functionString.FixCamelCaseAttributeNaming(attributeType) + ";");

            return context.GetVariable("__functionParsed__").As<Function>();
        }
    }
}
