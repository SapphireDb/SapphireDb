using Newtonsoft.Json.Linq;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RealtimeDatabase.Models.Prefilter
{
    class WherePrefilter : IPrefilter
    {
        public string CompareFunctionString { get; set; }

        public Dictionary<string, string> ContextData { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            if (array.Any())
            {
                try
                {
                    Func<object, bool> function = CompareFunctionString.CreateFunction(array.FirstOrDefault().GetType(), ContextData)
                        .MakeDelegate<Func<object, bool>>();

                    return array.Where(function);
                }
                catch
                {

                }
            }

            return array;
        }
    }
}
