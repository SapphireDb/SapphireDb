using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SapphireDb.Command;
using SapphireDb.Command.Query;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class CollectionHelper
    {
        public static ResponseBase GetCollection(SapphireDbContext db, QueryCommand command,
            HttpInformation information, IServiceProvider serviceProvider)
        {
            Type dbContextType = db.GetType();
            KeyValuePair<Type, string> property = dbContextType.GetDbSetType(command.CollectionName);

            if (property.Key != null)
            {
                if (!property.Key.CanQuery(information, serviceProvider))
                {
                    return command.CreateExceptionResponse<QueryResponse>(
                        "Not allowed to query values from collection");
                }
                
                IQueryable<object> collectionValues = db.GetCollectionValues(serviceProvider, information, property, command.Prefilters);

                QueryResponse queryResponse = new QueryResponse()
                {
                    ReferenceId = command.ReferenceId,
                };
                
                IAfterQueryPrefilter afterQueryPrefilter = command.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

                if (afterQueryPrefilter != null)
                {
                    afterQueryPrefilter.Initialize(property.Key);
                    queryResponse.Result = afterQueryPrefilter.Execute(collectionValues);
                }
                else
                {
                    IEnumerable<object> values = collectionValues.AsEnumerable();

                    AuthModelInfo authModelInfo = property.Key.GetAuthModelInfos();
                    
                    if (authModelInfo.QueryEntryAuthAttributes.Any())
                    {
                        values = values.Where(value => property.Key.CanQueryEntry(information, serviceProvider, value));
                    }

                    values = values.Select(v => v.GetAuthenticatedQueryModel(information, serviceProvider));
                    
                    queryResponse.Result = values.ToList();
                }

                return queryResponse;
            }

            return command.CreateExceptionResponse<QueryResponse>("No set for collection was found.");
        }
    }
}
