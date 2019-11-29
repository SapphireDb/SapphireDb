using System.Collections.Generic;
using System.Linq;


namespace SapphireDb.Internal.Prefilter
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
