using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using RealtimeDatabase.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Internal.Prefilter
{
    public class OrderByPrefilter : IPrefilter
    {
        public OrderByPrefilter()
        {
            engine = JsEngineSwitcher.Current.CreateDefaultEngine();
        }

        private readonly IJsEngine engine;

        public string SelectFunctionString { get; set; }

        public object[] ContextData { get; set; }

        public bool Descending { get; set; }

        private Func<object, IComparable> keySelector;

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                if (keySelector == null)
                {
                    keySelector = SelectFunctionString.CreatePredicateFunction(ContextData, engine);
                }

                return Descending ? array.OrderByDescending(keySelector) : array.OrderBy(keySelector);
            }

            return array;
        }

        public void Dispose()
        {
            engine.Dispose();
        }
    }
}
