using System.Collections.Generic;

namespace RealtimeDatabase.Models.Prefilter
{
    public interface IPrefilter
    {
        IEnumerable<object> Execute(IEnumerable<object> array);
    }
}
