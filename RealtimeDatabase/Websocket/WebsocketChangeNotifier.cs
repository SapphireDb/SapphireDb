using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    public class WebsocketChangeNotifier
    {
        private readonly WebsocketConnectionManager connectionManager;
        private readonly DbContextAccesor dbContextAccessor;
        private readonly IServiceProvider serviceProvider;

        public WebsocketChangeNotifier(WebsocketConnectionManager connectionManager, DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
        {
            this.connectionManager = connectionManager;
            this.dbContextAccessor = dbContextAccessor;
            this.serviceProvider = serviceProvider;
        }

        public void HandleChanges(List<ChangeResponse> changes)
        {
            RealtimeDbContext db = dbContextAccessor.GetContext();

            foreach (WebsocketConnection connection in connectionManager.connections)
            {
                foreach (IGrouping<string, CollectionSubscription> subscriptionGrouping in 
                    connection.Subscriptions.GroupBy(s => s.CollectionName))
                {
                    HandleSubscription(subscriptionGrouping, changes, connection, db);
                }
            }
        }

        private void HandleSubscription(IGrouping<string, CollectionSubscription> subscriptionGrouping, List<ChangeResponse> changes,
            WebsocketConnection connection, RealtimeDbContext db)
        {
            List<ChangeResponse> relevantChanges =
                        changes.Where(c => c.CollectionName == subscriptionGrouping.Key).ToList();

            KeyValuePair<Type, string> property = db.sets
                .FirstOrDefault(v => v.Value.ToLowerInvariant() == subscriptionGrouping.Key);

            if (property.Key != null)
            {
                relevantChanges = relevantChanges.Where(rc => property.Key.CanQuery(connection, rc.Value, serviceProvider)).ToList();

                IEnumerable<object> collectionSet = db.GetValues(property);

                foreach (CollectionSubscription cs in subscriptionGrouping)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    IEnumerable<object> currentCollectionSet = collectionSet;

                    foreach (IPrefilter prefilter in cs.Prefilters)
                    {
                        currentCollectionSet = prefilter.Execute(currentCollectionSet);
                    }

                    SendDataToClient(currentCollectionSet.ToList(), property, db, cs, relevantChanges, connection);
                }
            }
        }

        private void SendDataToClient(List<object> currentCollectionSetLoaded,
            KeyValuePair<Type, string> property, RealtimeDbContext db, CollectionSubscription cs, List<ChangeResponse> relevantChanges,
            WebsocketConnection connection)
        {
            List<object[]> currentCollectionPrimaryValues = new List<object[]>();

            foreach (object obj in currentCollectionSetLoaded)
            {
                SendRelevantFilesToClient(property, db, obj, currentCollectionPrimaryValues, cs, relevantChanges, connection);
            }

            foreach (object[] transmittedObject in cs.TransmittedData)
            {
                if (currentCollectionPrimaryValues.All(pks => pks.Except(transmittedObject).Any()))
                {
                    _ = connection.Send(new UnloadResponse
                    {
                        PrimaryValues = transmittedObject,
                        ReferenceId = cs.ReferenceId
                    });
                }
            }

            cs.TransmittedData = currentCollectionPrimaryValues;
        }

        private void SendRelevantFilesToClient(KeyValuePair<Type, string> property, RealtimeDbContext db, object obj,
            List<object[]> currentCollectionPrimaryValues, CollectionSubscription cs, List<ChangeResponse> relevantChanges,
            WebsocketConnection connection)
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
                    change.Value = change.Value.GetAuthenticatedQueryModel(connection, serviceProvider);
                    _ = connection.Send(change);
                }
            }
            else
            {
                _ = connection.Send(new LoadResponse
                {
                    NewObject = obj.GetAuthenticatedQueryModel(connection, serviceProvider),
                    ReferenceId = cs.ReferenceId
                });
            }
        }
    }
}
