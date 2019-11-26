using System.Collections.Generic;

namespace RealtimeDatabase.Internal.Prefilter
{
    public interface IPrefilter : IPrefilterBase
    {
        IEnumerable<object> Execute(IEnumerable<object> array);
    }
}
