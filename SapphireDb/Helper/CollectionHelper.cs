using System;
using System.Collections.Generic;
using System.Linq;
using SapphireDb.Attributes;
using SapphireDb.Command;
using SapphireDb.Command.Query;
using SapphireDb.Command.QueryQuery;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;
using SapphireDb.Models.SapphireApiBuilder;

namespace SapphireDb.Helper
{
    static class CollectionHelper
    {
        public static IQueryable<object> GetCollectionValues(this SapphireDbContext db,
            KeyValuePair<Type, string> property, List<IPrefilterBase> prefilters)
        {
            IQueryable<object> collectionSet = db.GetValues(property);

            foreach (IPrefilter prefilter in prefilters.OfType<IPrefilter>())
            {
                collectionSet = prefilter.Execute(collectionSet);
            }

            return collectionSet;
        }

        public static KeyValuePair<Type, string> GetCollectionType(SapphireDbContext db, IQueryCommand command)
        {
            Type dbContextType = db.GetType();
            KeyValuePair<Type, string> property = dbContextType.GetDbSetType(command.CollectionName);
            
            if (property.Key == null)
            {
                throw new CollectionNotFoundException(command.ContextName, command.CollectionName);
            }

            return property;
        }
        
        public static ResponseBase GetCollection(SapphireDbContext db, IQueryCommand command, KeyValuePair<Type, string> property, List<IPrefilterBase> prefilters,
            HttpInformation information, IServiceProvider serviceProvider)
        {
            if (!property.Key.CanQuery(information, serviceProvider))
            {
                throw new UnauthorizedException("Not allowed to query values from collection");
            }

            IQueryable<object> collectionValues = db.GetCollectionValues(property, prefilters);

            QueryResponse queryResponse = new QueryResponse()
            {
                ReferenceId = command.ReferenceId,
            };

            IAfterQueryPrefilter afterQueryPrefilter =
                prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

            if (afterQueryPrefilter != null)
            {
                queryResponse.Result = afterQueryPrefilter.Execute(collectionValues);
            }
            else
            {
                IEnumerable<object> values = collectionValues.AsEnumerable();

                ModelAttributesInfo modelAttributesInfo = property.Key.GetModelAttributesInfo();

                if (modelAttributesInfo.QueryEntryAuthAttributes.Any())
                {
                    values = values.Where(value => property.Key.CanQueryEntry(information, serviceProvider, value));
                }

                values = values.Select(v => v.GetAuthenticatedQueryModel(information, serviceProvider));

                queryResponse.Result = values.ToList();
            }

            return queryResponse;
        }

        public static List<IPrefilterBase> GetQueryPrefilters(KeyValuePair<Type, string> property, QueryQueryCommand queryCommand,
            HttpInformation information, IServiceProvider serviceProvider)
        {
            ModelAttributesInfo modelAttributesInfo = property.Key.GetModelAttributesInfo();
            QueryAttribute query = modelAttributesInfo.QueryAttributes
                .FirstOrDefault(q =>
                    q.QueryName.Equals(queryCommand.QueryName, StringComparison.InvariantCultureIgnoreCase));

            DefaultQueryAttribute defaultQuery = modelAttributesInfo.DefaultQueryAttributes.SingleOrDefault();
            
            if (query == null)
            {
                throw new QueryNotFoundException(queryCommand.ContextName, queryCommand.CollectionName, queryCommand.QueryName);
            }

            dynamic queryBuilder =
                Activator.CreateInstance(typeof(SapphireQueryBuilder<>).MakeGenericType(property.Key));

            if (defaultQuery != null)
            {
                if (defaultQuery.FunctionLambda != null)
                {
                    queryBuilder = defaultQuery.FunctionLambda(queryBuilder, information, queryCommand.Parameters);
                }
                else if (defaultQuery.FunctionInfo != null)
                {
                    queryBuilder = defaultQuery.FunctionInfo.Invoke(null,
                        defaultQuery.FunctionInfo.CreateParameters(information, serviceProvider, queryCommand.Parameters, (object)queryBuilder));
                }
            }
            
            if (query.FunctionLambda != null)
            {
                queryBuilder = query.FunctionLambda(queryBuilder, information, queryCommand.Parameters);
            }
            else if (query.FunctionInfo != null)
            {
                queryBuilder = query.FunctionInfo.Invoke(null,
                    query.FunctionInfo.CreateParameters(information, serviceProvider, queryCommand.Parameters, (object)queryBuilder));
            }

            List<IPrefilterBase> prefilters = typeof(SapphireQueryBuilderBase<>)
                .MakeGenericType(property.Key)
                .GetField("prefilters")?
                .GetValue(queryBuilder);
            
            prefilters.ForEach(prefilter => prefilter.Initialize(property.Key));

            return prefilters;
        }
    }
}