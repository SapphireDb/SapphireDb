using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireOrderedQueryBuilder<T> : SapphireQueryBuilder<T> where T : class
    {
        public SapphireOrderedQueryBuilder(List<IPrefilterBase> prefilters) : base(prefilters)
        {
            
        }
        
        public SapphireOrderedQueryBuilder<T> ThenOrderBy<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            ThenOrderByPrefilter thenOrderByPrefilter = new ThenOrderByPrefilter();
            thenOrderByPrefilter.InitializeServer(selector);
            prefilters.Add(thenOrderByPrefilter);
            return this;
        }
        
        public SapphireOrderedQueryBuilder<T> ThenOrderByDescending<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            ThenOrderByPrefilter thenOrderByPrefilter = new ThenOrderByPrefilter()
            {
                Descending = true
            };
            thenOrderByPrefilter.InitializeServer(selector);
            prefilters.Add(thenOrderByPrefilter);
            return this;
        }
    }
}