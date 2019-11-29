using System.Collections.Generic;

namespace SapphireDb.Internal.Prefilter
{
    public interface IPrefilter : IPrefilterBase
    {
        IEnumerable<object> Execute(IEnumerable<object> array);
    }
}
