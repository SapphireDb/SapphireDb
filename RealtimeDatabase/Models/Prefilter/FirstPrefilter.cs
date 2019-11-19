using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace RealtimeDatabase.Models.Prefilter
{
    public class FirstPrefilter : IAfterQueryPrefilter
    {
        public object Execute(IEnumerable<object> array)
        {
            return array.FirstOrDefault();
        }

        public void Dispose()
        {
            
        }
    }
}
