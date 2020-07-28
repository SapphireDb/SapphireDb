using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public interface IPrefilter : IPrefilterBase
    {
        IQueryable<object> Execute(IQueryable<object> array);
    }
}
