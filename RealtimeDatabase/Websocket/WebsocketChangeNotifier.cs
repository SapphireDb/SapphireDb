using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    public class WebsocketChangeNotifier
    {
        private readonly WebsocketConnectionManager connectionManager;
        private readonly DbContextAccesor dbContextAccesor;

        public WebsocketChangeNotifier(WebsocketConnectionManager _connectionManager, DbContextAccesor _dbContextAccesor)
        {
            connectionManager = _connectionManager;
            dbContextAccesor = _dbContextAccesor;
        }

        public async Task HandleChanges(List<ChangeResponse> changes)
        {
            RealtimeDbContext db = dbContextAccesor.GetContext();

            foreach (WebsocketConnection connection in connectionManager.connections)
            {
                foreach (IGrouping<string, CollectionSubscription> subscriptionGrouping in 
                    connection.Subscriptions.GroupBy(s => s.CollectionName))
                {
                    IEnumerable<ChangeResponse> relevantChanges =
                        changes.Where(c => c.CollectionName == subscriptionGrouping.Key);

                    KeyValuePair<Type, string> property = db.sets
                        .FirstOrDefault(v => v.Value.ToLowerInvariant() == subscriptionGrouping.Key);

                    if (property.Key != null)
                    {
                        if (!property.Key.IsAuthorized(connection, RealtimeAuthorizeAttribute.OperationType.Read))
                        {
                            continue;
                        }

                        IEnumerable<object> collectionSet = (IEnumerable<object>)db.GetType().GetProperty(property.Value).GetValue(db);

                        foreach (CollectionSubscription cs in subscriptionGrouping)
                        {
                            IEnumerable<object> currentCollectionSet = collectionSet;

                            foreach (IPrefilter prefilter in cs.Prefilters)
                            {
                                currentCollectionSet = prefilter.Execute(currentCollectionSet);
                            }

                            List<object> currentCollectionSetLoaded = currentCollectionSet.ToList();
                            List<object[]> currentCollectionPrimaryValues = new List<object[]>();

                            foreach (object obj in currentCollectionSetLoaded)
                            {
                                object[] primaryValues = property.Key.GetPrimaryKeyValues(db, obj);
                                currentCollectionPrimaryValues.Add(primaryValues);

                                bool clientHasObject = cs.TransmittedData.Any(pks => !pks.Except(primaryValues).Any());

                                if (clientHasObject)
                                {
                                    ChangeResponse change = relevantChanges
                                        .FirstOrDefault(c => !c.PrimaryValues.Except(primaryValues).Any());

                                    if (change != null)
                                    {
                                        change.ReferenceId = cs.ReferenceId;
                                        await connection.Websocket.Send(JsonHelper.Serialize(change));
                                    }
                                }
                                else
                                {
                                    await connection.Websocket.Send(JsonHelper.Serialize(new LoadResponse() {
                                        NewObject = obj,
                                        ReferenceId = cs.ReferenceId
                                    }));
                                }
                            }

                            foreach (object[] transmittedObject in cs.TransmittedData)
                            {
                                if (!currentCollectionPrimaryValues.Any(pks => !pks.Except(transmittedObject).Any()))
                                {
                                    await connection.Websocket.Send(JsonHelper.Serialize(new UnloadResponse() {
                                        PrimaryValues = transmittedObject,
                                        ReferenceId = cs.ReferenceId
                                    }));
                                }
                            }

                            cs.TransmittedData = currentCollectionPrimaryValues;
                        }
                    }
                }
            }
        }
    }
}
