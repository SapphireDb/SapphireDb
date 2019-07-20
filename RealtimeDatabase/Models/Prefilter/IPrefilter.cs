using System.Collections.Generic;

namespace RealtimeDatabase.Models.Prefilter
{
    public interface IPrefilter : IPrefilterBase
    {
        IEnumerable<object> Execute(IEnumerable<object> array);
    }
}
