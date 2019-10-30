using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    public class CountPrefilter : IAfterQueryPrefilter
    {
        public object Execute(IEnumerable<object> array)
        {
            return array.Count();
        }

        public void Dispose()
        {
            
        }
    }
}
