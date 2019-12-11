using System.Collections.Generic;
using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public interface IAfterQueryPrefilter : IPrefilterBase
    {
        object Execute(IQueryable<object> array);
    }
}
