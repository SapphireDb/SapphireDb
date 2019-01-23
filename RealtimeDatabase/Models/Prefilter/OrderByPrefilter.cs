using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    class OrderByPrefilter : IPrefilter
    {
        public string SelectFunctionString { get; set; }

        public Dictionary<string, string> ContextData { get; set; }

        public bool Descending { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                try
                {
                    Func<object, IComparable> function = SelectFunctionString.CreateFunction(array.FirstOrDefault()?.GetType(), ContextData)
                        .MakeDelegate<Func<object, IComparable>>();

                    if (Descending)
                    {
                        return array.OrderByDescending(function);
                    }
                    else
                    {
                        return array.OrderBy(function);
                    }
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
