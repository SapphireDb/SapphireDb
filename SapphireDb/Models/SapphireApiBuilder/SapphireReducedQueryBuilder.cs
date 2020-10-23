using System.Collections.Generic;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireReducedQueryBuilder<T> : SapphireQueryBuilderBase<T> where T : class
    {
        public SapphireReducedQueryBuilder(List<IPrefilterBase> prefilters) : base(prefilters)
        {
            
        }
    }
}