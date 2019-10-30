using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    public class SelectPrefilter : IAfterQueryPrefilter
    {
        public SelectPrefilter()
        {
            engine = JsEngineSwitcher.Current.CreateDefaultEngine();
        }

        private readonly IJsEngine engine;

        public string SelectFunctionString { get; set; }

        public object[] ContextData { get; set; }

        private Func<object, object> keySelector;

        public object Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                if (keySelector == null)
                {
                    keySelector = x =>
                    {
                        string result = SelectFunctionString.CreatePredicateFunction(ContextData, engine)(x);

                        try
                        {
                            return JToken.Parse(result);
                        }
                        catch
                        {
                            return result;
                        }
                    };
                }

                return array.Select(keySelector);
            }

            return array;
        }

        public void Dispose()
        {
            engine.Dispose();
        }
    }
}
