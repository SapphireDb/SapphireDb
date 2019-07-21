using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using RealtimeDatabase.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    public class OrderByPrefilter : IPrefilter
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

                return Descending ? array.OrderByDescending(keySelector) : array.OrderBy(keySelector);
            }

            return array;
        }
    }
}
