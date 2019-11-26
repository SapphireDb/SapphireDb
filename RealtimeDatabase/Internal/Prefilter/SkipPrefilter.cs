using System.Collections.Generic;
using System.Linq;

namespace RealtimeDatabase.Internal.Prefilter
{
    public class SkipPrefilter : IPrefilter
    {
        public int Number { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            return array.Skip(Number);
        }

        public void Dispose()
        {
            
        }
    }
}
