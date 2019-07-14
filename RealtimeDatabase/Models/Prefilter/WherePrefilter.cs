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

        private readonly Func<object, bool> predicate;

        public WherePrefilter()
        {
            try
            {
                Func<string, bool> parsedFunc = CompareFunctionString.CreateFunction(ContextData).MakeDelegate<Func<string, bool>>();
                predicate = dataObject => parsedFunc(JsonHelper.Serialize(dataObject));
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
                    return array.Where(predicate);
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
