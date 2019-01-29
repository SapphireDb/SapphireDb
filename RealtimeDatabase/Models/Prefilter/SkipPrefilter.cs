using System.Collections.Generic;
using System.Linq;

namespace RealtimeDatabase.Models.Prefilter
{
    public class SkipPrefilter : IPrefilter
    {
        public int Number { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            return array.Skip(Number);
        }
    }
}
