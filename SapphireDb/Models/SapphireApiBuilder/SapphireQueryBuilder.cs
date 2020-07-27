using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SapphireDb.Command.Subscribe;
using SapphireDb.Helper;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireQueryBuilder<T> where T : class
    {
        public List<IPrefilterBase> prefilters = new List<IPrefilterBase>();

        public SapphireQueryBuilder<T> Skip(int skipCount)
        {
            prefilters.Add(new SkipPrefilter()
            {
                Number = skipCount
            });
            
            return this;
        }
        
        public SapphireQueryBuilder<T> Take(int takeCount)
        {
            prefilters.Add(new TakePrefilter()
            {
                Number = takeCount
            });
            
            return this;
        }
        
        // Where
        
        public SapphireOrderedQueryBuilder<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            PropertyInfo property = (PropertyInfo)((MemberExpression) selector.Body).Member;
            
            prefilters.Add(new OrderByPrefilter()
            {
                Descending = false,
                Property = property.Name.ToCamelCase()
            });
            
            return new SapphireOrderedQueryBuilder<T>(prefilters);
        }
        
        public SapphireOrderedQueryBuilder<T> OrderByDescending<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            PropertyInfo property = (PropertyInfo)((MemberExpression) selector.Body).Member;
            
            prefilters.Add(new OrderByPrefilter()
            {
                Descending = true,
                Property = property.Name.ToCamelCase()
            });
            
            return new SapphireOrderedQueryBuilder<T>(prefilters);
        }
    }
}