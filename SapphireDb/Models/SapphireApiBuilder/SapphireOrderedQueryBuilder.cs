using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SapphireDb.Helper;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireOrderedQueryBuilder<T> : SapphireQueryBuilder<T> where T : class
    {
        public SapphireOrderedQueryBuilder(List<IPrefilterBase> prefilters)
        {
            this.prefilters = prefilters;
        }
        
        public SapphireOrderedQueryBuilder<T> ThenOrderBy<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            PropertyInfo property = (PropertyInfo)((MemberExpression) selector.Body).Member;
            
            prefilters.Add(new ThenOrderByPrefilter()
            {
                Descending = false,
                Property = property.Name.ToCamelCase()
            });
            
            return this;
        }
        
        public SapphireOrderedQueryBuilder<T> ThenOrderByDescending<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            PropertyInfo property = (PropertyInfo)((MemberExpression) selector.Body).Member;
            
            prefilters.Add(new ThenOrderByPrefilter()
            {
                Descending = true,
                Property = property.Name.ToCamelCase()
            });
            
            return this;
        }
    }
}