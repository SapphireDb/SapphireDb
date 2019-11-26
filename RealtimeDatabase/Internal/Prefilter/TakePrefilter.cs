using System.Collections.Generic;
using System.Linq;

namespace RealtimeDatabase.Internal.Prefilter
{
    public class TakePrefilter : IPrefilter
    {
        public int Number { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            return array.Take(Number);
        }

        public void Dispose()
        {
            
        }
    }
}
