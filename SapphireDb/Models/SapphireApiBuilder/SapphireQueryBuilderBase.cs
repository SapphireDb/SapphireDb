using System.Collections.Generic;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireQueryBuilderBase<T> where T : class
    {
        public List<IPrefilterBase> prefilters;

        public SapphireQueryBuilderBase()
        {
            prefilters = new List<IPrefilterBase>();
        }
        
        public SapphireQueryBuilderBase(List<IPrefilterBase> prefilters)
        {
            this.prefilters = prefilters;
        }
    }
}