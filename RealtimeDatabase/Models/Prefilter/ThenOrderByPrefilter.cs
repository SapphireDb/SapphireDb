using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using RealtimeDatabase.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    public class ThenOrderByPrefilter : IPrefilter
    {
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
                    keySelector = SelectFunctionString.CreatePredicateFunction(ContextData);
                }

                IOrderedEnumerable<object> orderedArray = (IOrderedEnumerable<object>)array;
                return Descending ? orderedArray.ThenByDescending(keySelector) : orderedArray.ThenBy(keySelector);
            }

            return array;
        }
    }
}
