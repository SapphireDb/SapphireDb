using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealtimeDatabase.Models.Prefilter
{
    class SkipPrefilter : IPrefilter
    {
        public int Number { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            return array.Skip(Number);
        }
    }
}
