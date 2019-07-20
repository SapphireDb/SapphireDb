using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealtimeDatabase.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    public class WherePrefilter : IPrefilter
    {
        public string CompareFunctionString { get; set; }

        public object[] ContextData { get; set; }

        private Func<object, bool> predicate;

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                if (predicate == null)
                {
                    predicate = CompareFunctionString.CreateBoolFunction(ContextData);
                }

                return array.Where(predicate);
            }

            return array;
        }
    }
}
