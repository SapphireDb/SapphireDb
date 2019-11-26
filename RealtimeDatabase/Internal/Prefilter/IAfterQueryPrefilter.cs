using System.Collections.Generic;

namespace RealtimeDatabase.Internal.Prefilter
{
    public interface IAfterQueryPrefilter : IPrefilterBase
    {
        object Execute(IEnumerable<object> array);
    }
}
