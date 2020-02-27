using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public class ThenOrderByPrefilter : OrderByPrefilter
    {
        public override IQueryable<object> Execute(IQueryable<object> array)
        {
            return Descending
                ? ((IOrderedQueryable<object>)array).ThenByDescending(PropertySelectExpression)
                : ((IOrderedQueryable<object>)array).ThenBy(PropertySelectExpression);
        }
        
        public new string Hash()
        {
            return $"ThenOrderByPrefilter,{Property},{Descending}";
        }
    }
}
