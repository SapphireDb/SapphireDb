using System.Collections.Generic;
using System.Linq;


namespace SapphireDb.Internal.Prefilter
{
    public class LastPrefilter: IAfterQueryPrefilter
    {
        public object Execute(IEnumerable<object> array)
        {
            return array.LastOrDefault();
        }

        public void Dispose()
        {
            
        }
    }
}
