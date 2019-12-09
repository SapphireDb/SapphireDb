using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SapphireDb.Command;
using SapphireDb.Command.Query;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection.Websocket;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Connection
{
    public class SapphireChangeNotifier
    {
        private readonly ConnectionManager connectionManager;
        private readonly DbContextAccesor dbContextAccessor;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<WebsocketConnection> logger;
        private readonly DbContextTypeContainer contextTypeContainer;

        public SapphireChangeNotifier(
            ConnectionManager connectionManager,
            DbContextAccesor dbContextAccessor,
            IServiceProvider serviceProvider,
            ILogger<WebsocketConnection> logger,
            DbContextTypeContainer contextTypeContainer)
        {
            this.connectionManager = connectionManager;
            this.dbContextAccessor = dbContextAccessor;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.contextTypeContainer = contextTypeContainer;
        }

        public void HandleChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            IEnumerable<SubscriptionConnectionMapping> subscriptions = connectionManager.connections
                .SelectMany(c => c.Subscriptions.Select(s => new SubscriptionConnectionMapping() { Subscription = s, Connection = c}));

            string contextName = contextTypeContainer.GetName(dbContextType);

            IEnumerable<IGrouping<string, SubscriptionConnectionMapping>> subscriptionGroupings =
                subscriptions
                    .Where(s => s.Subscription.ContextName == contextName)
                    .GroupBy(s => s.Subscription.CollectionName);

            foreach (IGrouping<string, SubscriptionConnectionMapping> subscriptionGrouping in subscriptionGroupings)
            {
                List<ChangeResponse> relevantChanges = changes.Where(c => c.CollectionName == subscriptionGrouping.Key).ToList();

                if (!relevantChanges.Any())
                {
                    continue;
                }

                foreach (IGrouping<ConnectionBase, SubscriptionConnectionMapping> connectionGrouping in subscriptionGrouping.GroupBy(s => s.Connection))
                {
                    Task.Run(() =>
                    {
                        IServiceProvider requestServiceProvider = null;

                        try
                        {
                            requestServiceProvider = connectionGrouping.Key.HttpContext?.RequestServices;
                        }
                        catch (ObjectDisposedException)
                        {
                            connectionManager.RemoveConnection(connectionGrouping.Key);
                            return;
                        }

                        SapphireDbContext db = dbContextAccessor.GetContext(dbContextType, requestServiceProvider);
                        KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == subscriptionGrouping.Key);

                        List<object> collectionSet = db.GetValues(property, serviceProvider, connectionGrouping.Key.Information).ToList();

                        List<ChangeResponse> changesForConnection = relevantChanges.Where(rc => property.Key.CanQuery(connectionGrouping.Key.Information, rc.Value, serviceProvider)).ToList();

                        foreach (SubscriptionConnectionMapping mapping in connectionGrouping)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    HandleSubscription(mapping, changesForConnection, db, property.Key, collectionSet);
                                }
                                catch (Exception ex)
                                {
                                    SubscribeCommand tempErrorCommand = new SubscribeCommand()
                                    {
                                        CollectionName = subscriptionGrouping.Key,
                                        ReferenceId = mapping.Subscription.ReferenceId,
                                        Prefilters = mapping.Subscription.Prefilters
                                    };

                                    _ = mapping.Connection.Send(tempErrorCommand.CreateExceptionResponse<ResponseBase>(ex));
                                    logger.LogError($"Error handling subscription '{mapping.Subscription.ReferenceId}' of {subscriptionGrouping.Key}");
                                    logger.LogError(ex.Message);
                                }
                            });
                        }
                    });
                }
            }
        }

        private void HandleSubscription(SubscriptionConnectionMapping mapping, List<ChangeResponse> changes, 
            SapphireDbContext db, Type modelType, IEnumerable<object> collectionSet)
        {
            mapping.Subscription.Lock.Wait();

            try
            {
                IEnumerable<object> currentCollectionSet = collectionSet;

                foreach (IPrefilter prefilter in mapping.Subscription.Prefilters.OfType<IPrefilter>())
                {
                    currentCollectionSet = prefilter.Execute(currentCollectionSet);
                }

                IAfterQueryPrefilter afterQueryPrefilter =
                    mapping.Subscription.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

                if (afterQueryPrefilter != null)
                {
                    List<object> result = currentCollectionSet.Where(v =>
                            modelType.CanQuery(mapping.Connection.Information, v, serviceProvider))
                        .Select(v => v.GetAuthenticatedQueryModel(mapping.Connection.Information, serviceProvider))
                        .ToList();

                    _ = mapping.Connection.Send(new QueryResponse()
                    {
                        ReferenceId = mapping.Subscription.ReferenceId,
                        Result = afterQueryPrefilter.Execute(result)
                    });
                }
                else
                {
                    SendDataToClient(currentCollectionSet.ToList(), modelType, db, mapping, changes);
                }
            }
            finally
            {
                mapping.Subscription.Lock.Release();
            }
        }

        private void SendDataToClient(List<object> currentCollectionSetLoaded,
            Type modelType, SapphireDbContext db, SubscriptionConnectionMapping mapping, List<ChangeResponse> relevantChanges)
        {
            List<object[]> currentCollectionPrimaryValues = new List<object[]>();

            foreach (object obj in currentCollectionSetLoaded)
            {
                SendRelevantFilesToClient(modelType, db, obj, currentCollectionPrimaryValues, mapping, relevantChanges);
            }

            foreach (object[] transmittedObject in mapping.Subscription.TransmittedData)
            {
                if (currentCollectionPrimaryValues.All(pks => pks.Except(transmittedObject).Any()))
                {
                    _ = mapping.Connection.Send(new UnloadResponse
                    {
                        PrimaryValues = transmittedObject,
                        ReferenceId = mapping.Subscription.ReferenceId
                    });
                }
            }

            mapping.Subscription.TransmittedData = currentCollectionPrimaryValues;
        }

        private void SendRelevantFilesToClient(Type modelType, SapphireDbContext db, object obj,
            List<object[]> currentCollectionPrimaryValues, SubscriptionConnectionMapping mapping, List<ChangeResponse> relevantChanges)
        {
            object[] primaryValues = modelType.GetPrimaryKeyValues(db, obj);
            currentCollectionPrimaryValues.Add(primaryValues);

            bool clientHasObject = mapping.Subscription.TransmittedData.Any(pks => !pks.Except(primaryValues).Any());

            if (clientHasObject)
            {
                ChangeResponse change = relevantChanges
                    .FirstOrDefault(c => c.State == ChangeResponse.ChangeState.Modified && !c.PrimaryValues.Except(primaryValues).Any());

                if (change != null)
                {
                    object value = change.Value.GetAuthenticatedQueryModel(mapping.Connection.Information, serviceProvider);
                    _ = mapping.Connection.Send(change.CreateResponse(mapping.Subscription.ReferenceId, value));
                }
            }
            else
            {
                _ = mapping.Connection.Send(new LoadResponse
                {
                    NewObject = obj.GetAuthenticatedQueryModel(mapping.Connection.Information, serviceProvider),
                    ReferenceId = mapping.Subscription.ReferenceId
                });
            }
        }
    }
}