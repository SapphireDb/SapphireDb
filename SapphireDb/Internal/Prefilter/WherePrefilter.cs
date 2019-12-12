using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using SapphireDb.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace SapphireDb.Internal.Prefilter
{
    public class WherePrefilter : IPrefilter
    {
        public WherePrefilter()
        {
            engine = JsEngineSwitcher.Current.CreateDefaultEngine();
        }

        public bool Initialized { get; set; } = true;

        public void Initialize(Type modelType) { }

        private readonly IJsEngine engine;

        public string CompareFunctionString { get; set; }

        public object[] ContextData { get; set; }

        private Func<object, bool> predicate;

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            //if (array.Any())
            //{
            //    if (predicate == null)
            //    {
            //        predicate = CompareFunctionString.CreateBoolFunction(ContextData, engine);
            //    }

            //    return array.Where(predicate);
            //}

            return array;
        }

        public void Dispose()
        {
            engine.Dispose();
        }
    }
}
