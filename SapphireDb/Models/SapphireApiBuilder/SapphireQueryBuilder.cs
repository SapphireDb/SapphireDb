using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireQueryBuilder<T> : SapphireQueryBuilderBase<T> where T : class
    {
        public SapphireQueryBuilder()
        {
            
        }
        
        public SapphireQueryBuilder(List<IPrefilterBase> prefilters) : base(prefilters)
        {
            
        }
        
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

        public SapphireQueryBuilder<T> Where(Expression<Func<T, bool>> whereExpression)
        {
            WherePrefilter wherePrefilter = new WherePrefilter();
            wherePrefilter.InitializeServer(whereExpression);
            prefilters.Add(wherePrefilter);
            return this;
        }
        
        public SapphireOrderedQueryBuilder<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            OrderByPrefilter orderByPrefilter = new OrderByPrefilter();
            orderByPrefilter.InitializeServer(selector);
            prefilters.Add(orderByPrefilter);
            return new SapphireOrderedQueryBuilder<T>(prefilters);
        }
        
        public SapphireOrderedQueryBuilder<T> OrderByDescending<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            OrderByPrefilter orderByPrefilter = new OrderByPrefilter()
            {
                Descending = true
            };
            orderByPrefilter.InitializeServer(selector);
            prefilters.Add(orderByPrefilter);
            return new SapphireOrderedQueryBuilder<T>(prefilters);
        }
        
        public SapphireReducedQueryBuilder<T> Select(params string[] properties)
        {
            prefilters.Add(new SelectPrefilter()
            {
                Properties = properties.ToList()
            });
            
            return new SapphireReducedQueryBuilder<T>(prefilters);
        }
        
        public SapphireQueryBuilder<T> Include(string include)
        {
            prefilters.Add(new IncludePrefilter()
            {
                Include = include
            });

            return this;
        }
        
        public SapphireReducedQueryBuilder<T> Count()
        {
            prefilters.Add(new CountPrefilter());
            return new SapphireReducedQueryBuilder<T>(prefilters);
        }
        
        public SapphireReducedQueryBuilder<T> First()
        {
            prefilters.Add(new FirstPrefilter());
            return new SapphireReducedQueryBuilder<T>(prefilters);
        }
        
        public SapphireReducedQueryBuilder<T> Last()
        {
            prefilters.Add(new LastPrefilter());
            return new SapphireReducedQueryBuilder<T>(prefilters);
        }
    }
}