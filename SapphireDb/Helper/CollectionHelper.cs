using System;
using System.Collections.Generic;
using System.Linq;
using SapphireDb.Command;
using SapphireDb.Command.Query;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

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
    }
}