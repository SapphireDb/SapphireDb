using System.Collections.Generic;
using System.Linq;


namespace SapphireDb.Internal.Prefilter
{
    public class LastPrefilter: IAfterQueryPrefilter
    {
        public object Execute(IQueryable<object> array)
        {
            return array.LastOrDefault();
        }

        public void Dispose()
        {
            
        }
    }
}
