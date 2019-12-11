using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using SapphireDb.Attributes;
using SapphireDb.Command;
using SapphireDb.Command.Query;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection.Websocket;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

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
            string contextName = contextTypeContainer.GetName(dbContextType);

            foreach (ConnectionBase connection in connectionManager.connections)
            {
                IServiceProvider requestServiceProvider = null;

                try
                {
                    requestServiceProvider = connection.HttpContext?.RequestServices;
                }
                catch (ObjectDisposedException)
                {
                    connectionManager.RemoveConnection(connection);
                    return;
                }

                requestServiceProvider ??= serviceProvider;

                IEnumerable<IGrouping<string, CollectionSubscription>> subscriptionGroupings = connection.Subscriptions
                    .Where(s => s.ContextName == contextName).GroupBy(s => s.CollectionName);

                foreach (IGrouping<string, CollectionSubscription> subscriptionGrouping in subscriptionGroupings)
                {
                    KeyValuePair<Type, string> property = dbContextType.GetDbSetType(subscriptionGrouping.Key);

                    List<ChangeResponse> subscriptionChanges = changes
                        .Where(c => c.CollectionName == subscriptionGrouping.Key).ToList();

                    QueryFunctionAttribute queryFunctionAttribute = property.Key.GetCustomAttribute<QueryFunctionAttribute>();
                    if (queryFunctionAttribute != null)
                    {
                        var queryFunctionInfo = property.Key.GetMethod(queryFunctionAttribute.Function, BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                        if (queryFunctionInfo != null)
                        {
                            object[] methodParameters = queryFunctionInfo.CreateParameters(connection.Information, serviceProvider);
                            dynamic queryFunctionExpression = ((dynamic)queryFunctionInfo.Invoke(null, methodParameters)).Compile();

                            subscriptionChanges = subscriptionChanges.Where(change => queryFunctionExpression(change.Value)).ToList();
                        }
                    }

                    if (!subscriptionChanges.Any())
                    {
                        continue;
                    }

                    foreach (CollectionSubscription subscription in subscriptionGrouping)
                    {
                        try
                        {
                            if (subscription.Prefilters.Any(prefilter => prefilter is IAfterQueryPrefilter || prefilter is TakePrefilter || prefilter is SkipPrefilter))
                            {
                                SapphireDbContext db = dbContextAccessor.GetContext(dbContextType, requestServiceProvider);

                                IQueryable<object> collectionValues = db.GetCollectionValues(requestServiceProvider, connection.Information, property,
                                    subscription.Prefilters);

                                IAfterQueryPrefilter afterQueryPrefilter =
                                    subscription.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

                                if (afterQueryPrefilter != null)
                                {
                                    _ = connection.Send(new QueryResponse()
                                    {
                                        ReferenceId = subscription.ReferenceId,
                                        Result = afterQueryPrefilter.Execute(collectionValues)
                                    });
                                }
                                else
                                {
                                    _ = connection.Send(new QueryResponse()
                                    {
                                        ReferenceId = subscription.ReferenceId,
                                        Result = collectionValues.ToList().Select(v => v.GetAuthenticatedQueryModel(connection.Information, requestServiceProvider))
                                    });
                                }
                            }
                            else
                            {
                                changes.ForEach(change =>
                                {
                                    object value = change.Value.GetAuthenticatedQueryModel(connection.Information, serviceProvider);
                                    _ = connection.Send(change.CreateResponse(subscription.ReferenceId, value));
                                });

                                //SendDataToClient(collection.ToList(), property.Key, db, mapping, changes);
                            }
                        }
                        catch (Exception ex)
                        {
                            SubscribeCommand tempErrorCommand = new SubscribeCommand()
                            {
                                CollectionName = subscriptionGrouping.Key,
                                ReferenceId = subscription.ReferenceId,
                                Prefilters = subscription.Prefilters
                            };

                            _ = connection.Send(tempErrorCommand.CreateExceptionResponse<ResponseBase>(ex));
                            logger.LogError($"Error handling subscription '{subscription.ReferenceId}' of {subscriptionGrouping.Key}");
                            logger.LogError(ex.Message);
                        }
                    }
                }
            }
        }

        //private void HandleSubscription(SubscriptionConnectionMapping mapping, List<ChangeResponse> changes, 
        //    SapphireDbContext db, KeyValuePair<Type, string> property, IServiceProvider serverProvider)
        //{
        //    mapping.Subscription.Lock.Wait();

        //    try
        //    {
        //        IQueryable<object> collection = db.GetCollectionValues(serverProvider, mapping.Connection.Information,
        //            property, mapping.Subscription.Prefilters);

        //        IAfterQueryPrefilter afterQueryPrefilter =
        //            subscription.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

        //        if (afterQueryPrefilter != null)
        //        {
        //            _ = mapping.Connection.Send(new QueryResponse()
        //            {
        //                ReferenceId = mapping.Subscription.ReferenceId,
        //                Result = afterQueryPrefilter.Execute(collection)
        //            });
        //        }
        //        else
        //        {
        //            SendDataToClient(collection.ToList(), property.Key, db, mapping, changes);
        //        }
        //    }
        //    finally
        //    {
        //        mapping.Subscription.Lock.Release();
        //    }
        //}

        //private void SendDataToClient(List<object> collectionValues,
        //    Type modelType, SapphireDbContext db, SubscriptionConnectionMapping mapping, List<ChangeResponse> changes)
        //{
        //    List<object[]> currentCollectionPrimaryValues = collectionValues.Select((value) => SendRelevantFilesToClient(modelType, db, value, mapping, changes)).ToList();

        //    foreach (object[] transmittedObject in mapping.Subscription.TransmittedData)
        //    {
        //        if (currentCollectionPrimaryValues.All(pks => pks.Except(transmittedObject).Any()))
        //        {
        //            _ = mapping.Connection.Send(new UnloadResponse
        //            {
        //                PrimaryValues = transmittedObject,
        //                ReferenceId = mapping.Subscription.ReferenceId
        //            });
        //        }
        //    }

        //    mapping.Subscription.TransmittedData = currentCollectionPrimaryValues;
        //}

        //private object[] SendRelevantFilesToClient(Type modelType, SapphireDbContext db, object obj, SubscriptionConnectionMapping mapping, List<ChangeResponse> relevantChanges)
        //{
        //    object[] primaryValues = modelType.GetPrimaryKeyValues(db, obj);

        //    bool clientHasObject = mapping.Subscription.TransmittedData.Any(pks => !pks.Except(primaryValues).Any());

        //    if (clientHasObject)
        //    {
        //        ChangeResponse change = relevantChanges
        //            .FirstOrDefault(c => c.State == ChangeResponse.ChangeState.Modified && !c.PrimaryValues.Except(primaryValues).Any());

        //        if (change != null)
        //        {
        //            object value = change.Value.GetAuthenticatedQueryModel(mapping.Connection.Information, serviceProvider);
        //            _ = mapping.Connection.Send(change.CreateResponse(mapping.Subscription.ReferenceId, value));
        //        }
        //    }
        //    else
        //    {
        //        _ = mapping.Connection.Send(new LoadResponse
        //        {
        //            NewObject = obj.GetAuthenticatedQueryModel(mapping.Connection.Information, serviceProvider),
        //            ReferenceId = mapping.Subscription.ReferenceId
        //        });
        //    }

        //    return primaryValues;
        //}
    }
}