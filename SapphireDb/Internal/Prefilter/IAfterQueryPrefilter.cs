using System.Collections.Generic;

namespace SapphireDb.Internal.Prefilter
{
    public interface IAfterQueryPrefilter : IPrefilterBase
    {
        object Execute(IEnumerable<object> array);
    }
}
