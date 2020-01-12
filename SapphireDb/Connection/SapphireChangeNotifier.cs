using System;
using System.Collections;
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
                Task.Run(() =>
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

                    HandleCollections(connection, contextName, dbContextType, changes, requestServiceProvider);
                });
            }
        }

        public void HandleCollections(ConnectionBase connection, string contextName, Type dbContextType,
            List<ChangeResponse> changes, IServiceProvider requestServiceProvider)
        {
            IEnumerable<IGrouping<string, CollectionSubscription>> subscriptionGroupings = connection.Subscriptions
                .Where(s => s.ContextName == contextName)
                .GroupBy(s => s.CollectionName);

            foreach (IGrouping<string, CollectionSubscription> subscriptionGrouping in subscriptionGroupings)
            {
                KeyValuePair<Type, string> property = dbContextType.GetDbSetType(subscriptionGrouping.Key);

                List<ChangeResponse> changesForCollection = changes
                    .Where(c => c.CollectionName == subscriptionGrouping.Key)
                    .ToList();

                ModelAttributesInfo modelAttributesInfo = property.Key.GetModelAttributesInfo();

                IEnumerable<ChangeResponse> authenticatedChanges = changesForCollection;

                if (modelAttributesInfo.QueryEntryAuthAttributes.Any())
                {
                    authenticatedChanges = changesForCollection
                        .Where(change => change.State == ChangeResponse.ChangeState.Deleted ||
                                         property.Key.CanQueryEntry(connection.Information, requestServiceProvider,
                                             change.Value));


                    IEnumerable<ChangeResponse> oldLoadedNotAllowed = changesForCollection
                        .Where(change => change.State == ChangeResponse.ChangeState.Modified &&
                                         !property.Key.CanQueryEntry(connection.Information, requestServiceProvider,
                                             change.Value))
                        .Select(change =>
                        {
                            ChangeResponse newChangeResponse = change.CreateResponse(null, change.Value);
                            newChangeResponse.State = ChangeResponse.ChangeState.Deleted;
                            return newChangeResponse;
                        });

                    authenticatedChanges = authenticatedChanges.Concat(oldLoadedNotAllowed);
                }

                List<ChangeResponse> collectionChanges = authenticatedChanges.ToList();

                if (collectionChanges.Any())
                {
                    if (modelAttributesInfo.QueryFunctionAttribute != null &&
                        modelAttributesInfo.QueryFunctionAttribute.FunctionInfo != null)
                    {
                        object[] methodParameters =
                            modelAttributesInfo.QueryFunctionAttribute.FunctionInfo.CreateParameters(connection.Information, serviceProvider);
                        dynamic queryFunctionExpression =
                            ((dynamic) modelAttributesInfo.QueryFunctionAttribute.FunctionInfo.Invoke(null, methodParameters)).Compile();

                        collectionChanges = collectionChanges.Where(change => queryFunctionExpression(change.Value))
                            .ToList();
                    }
                }

                foreach (CollectionSubscription subscription in subscriptionGrouping)
                {
                    Task.Run(() =>
                    {
                        HandleSubscription(subscription, dbContextType, requestServiceProvider, connection,
                            property, collectionChanges, changes);
                    });
                }
            }
        }

        public void HandleSubscription(CollectionSubscription subscription, Type dbContextType,
            IServiceProvider requestServiceProvider, ConnectionBase connection, KeyValuePair<Type, string> property,
            List<ChangeResponse> collectionChanges, List<ChangeResponse> allChanges)
        {
            try
            {
                bool anyCollectionChanges = collectionChanges.Any();

                if ((anyCollectionChanges && subscription.Prefilters.Any(prefilter =>
                         prefilter is IAfterQueryPrefilter || prefilter is TakePrefilter || prefilter is SkipPrefilter))
                    || HasIncludePrefilterWithChange(subscription, allChanges))
                {
                    SapphireDbContext db = dbContextAccessor.GetContext(dbContextType, requestServiceProvider);

                    IQueryable<object> collectionValues = db.GetCollectionValues(requestServiceProvider,
                        connection.Information, property, subscription.Prefilters);

                    IAfterQueryPrefilter afterQueryPrefilter =
                        subscription.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

                    if (afterQueryPrefilter != null)
                    {
                        afterQueryPrefilter.Initialize(property.Key);

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
                            Result = collectionValues
                                .AsEnumerable()
                                .Select(v => v.GetAuthenticatedQueryModel(connection.Information, serviceProvider))
                                .ToList()
                        });
                    }
                }
                else if (anyCollectionChanges)
                {
                    IEnumerable<WherePrefilter> wherePrefilters = subscription.Prefilters.OfType<WherePrefilter>();

                    foreach (WherePrefilter wherePrefilter in wherePrefilters)
                    {
                        wherePrefilter.Initialize(property.Key);
                        Func<object, bool> whereFunction = wherePrefilter.WhereExpression.Compile();
                        collectionChanges = collectionChanges.Where((change) => whereFunction(change.Value)).ToList();
                    }

                    collectionChanges.ForEach(change =>
                    {
                        object value =
                            change.Value.GetAuthenticatedQueryModel(connection.Information, requestServiceProvider);
                        _ = connection.Send(change.CreateResponse(subscription.ReferenceId, value));
                    });
                }
            }
            catch (Exception ex)
            {
                SubscribeCommand tempErrorCommand = new SubscribeCommand()
                {
                    CollectionName = subscription.CollectionName,
                    ReferenceId = subscription.ReferenceId,
                    Prefilters = subscription.Prefilters
                };

                _ = connection.Send(tempErrorCommand.CreateExceptionResponse<ResponseBase>(ex));
                logger.LogError(
                    $"Error handling subscription '{subscription.ReferenceId}' of {subscription.CollectionName}");
                logger.LogError(ex.Message);
            }
        }

        private bool HasIncludePrefilterWithChange(CollectionSubscription subscription, List<ChangeResponse> allChanges)
        {
            List<IncludePrefilter> includePrefilters = subscription.Prefilters.OfType<IncludePrefilter>().ToList();

            if (!includePrefilters.Any())
            {
                return false;
            }

            List<string> affectedCollections = includePrefilters
                .SelectMany(prefilter => prefilter.AffectedCollectionNames)
                .Distinct()
                .ToList();

            return allChanges.Any(change =>
                       change.CollectionName.Equals(subscription.CollectionName,
                           StringComparison.InvariantCultureIgnoreCase)) ||
                   affectedCollections.Any(collectionName => allChanges.Any(change =>
                       change.CollectionName.Equals(collectionName, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}