using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    public class SelectPrefilter : IAfterQueryPrefilter
    {
        public string SelectFunctionString { get; set; }

        public object[] ContextData { get; set; }

        private Func<object, object> keySelector;

        public object Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                if (keySelector == null)
                {
                    keySelector = x =>
                    {
                        string result = SelectFunctionString.CreatePredicateFunction(ContextData)(x);

                        try
                        {
                            return JToken.Parse(result);
                        }
                        catch
                        {
                            return result;
                        }
                    };
                }

                return array.Select(keySelector);
            }

            return array;
        }
    }
}
