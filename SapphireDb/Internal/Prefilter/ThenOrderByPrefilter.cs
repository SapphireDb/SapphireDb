using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using SapphireDb.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace SapphireDb.Internal.Prefilter
{
    public class ThenOrderByPrefilter : IPrefilter
    {
        public ThenOrderByPrefilter()
        {
            engine = JsEngineSwitcher.Current.CreateDefaultEngine();
        }

        private readonly IJsEngine engine;


        public string SelectFunctionString { get; set; }

        public object[] ContextData { get; set; }

        public bool Descending { get; set; }

        private Func<object, IComparable> keySelector;

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            //if (array.Any())
            //{
            //    if (keySelector == null)
            //    {
            //        keySelector = SelectFunctionString.CreatePredicateFunction(ContextData, engine);
            //    }

            //    IOrderedEnumerable<object> orderedArray = (IOrderedEnumerable<object>)array;
            //    return Descending ? orderedArray.ThenByDescending(keySelector) : orderedArray.ThenBy(keySelector);
            //}

            return array;
        }

        public void Dispose()
        {
            engine.Dispose();
        }
    }
}
