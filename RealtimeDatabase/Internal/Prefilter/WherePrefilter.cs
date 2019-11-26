using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using RealtimeDatabase.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Internal.Prefilter
{
    public class WherePrefilter : IPrefilter
    {
        public WherePrefilter()
        {
            engine = JsEngineSwitcher.Current.CreateDefaultEngine();
        }

        private readonly IJsEngine engine;

        public string CompareFunctionString { get; set; }

        public object[] ContextData { get; set; }

        private Func<object, bool> predicate;

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                if (predicate == null)
                {
                    predicate = CompareFunctionString.CreateBoolFunction(ContextData, engine);
                }

                return array.Where(predicate);
            }

            return array;
        }

        public void Dispose()
        {
            engine.Dispose();
        }
    }
}
