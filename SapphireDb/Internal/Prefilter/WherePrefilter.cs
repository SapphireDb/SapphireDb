using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using Newtonsoft.Json.Linq;
using SapphireDb.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace SapphireDb.Internal.Prefilter
{
    public class WherePrefilter : IPrefilter
    {
        public List<JToken> Conditions { get; set; }

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

        private bool initialized = false;

        public void Initialize(Type modelType)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
        }

        public void Dispose()
        {
            
        }
    }
}
