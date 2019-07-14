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

        private readonly Func<object, IComparable> keySelector;

        public ThenOrderByPrefilter()
        {
            try
            {
                Func<string, bool> parsedFunc = SelectFunctionString.CreateFunction(ContextData).MakeDelegate<Func<string, bool>>();
                keySelector = dataObject => parsedFunc(JsonHelper.Serialize(dataObject));
            }
            catch
            {
                // ignored
            }
        }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                try
                {
                    IOrderedEnumerable<object> orderedArray = (IOrderedEnumerable<object>)array;
                    return Descending ? orderedArray.ThenByDescending(keySelector) : orderedArray.ThenBy(keySelector);
                }
                catch
                {
                    // ignored
                }
            }

            return array;
        }
    }
}
