using System.Collections.Generic;
using System.Linq;


namespace RealtimeDatabase.Internal.Prefilter
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
