using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly DbContextAccesor dbContextAccessor;
        private readonly IServiceProvider serviceProvider;
        private readonly DbContextTypeContainer contextTypeContainer;
        private readonly SubscriptionManager subscriptionManager;

        public SapphireChangeNotifier(
            DbContextAccesor dbContextAccessor,
            IServiceProvider serviceProvider,
            DbContextTypeContainer contextTypeContainer,
            SubscriptionManager subscriptionManager)
        {
            this.dbContextAccessor = dbContextAccessor;
            // Test if working
            this.serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            // this.serviceProvider = serviceProvider;
            this.contextTypeContainer = contextTypeContainer;
            this.subscriptionManager = subscriptionManager;
        }

        public void HandleChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            string contextName = contextTypeContainer.GetName(dbContextType);

            Parallel.ForEach(changes.GroupBy(change => change.CollectionName),
                collectionChanges =>
                {
                    Task.Run(() =>
                    {
                        HandleChangesOfCollection(dbContextType, collectionChanges, contextName, changes);
                    });
                });

            // Parallel.ForEach(connectionManager.connections.Values, connection =>
            // {
            //     Task.Run(() =>
            //     {
            //         IServiceProvider requestServiceProvider = null;
            //
            //         try
            //         {
            //             requestServiceProvider = connection.HttpContext?.RequestServices;
            //         }
            //         catch (ObjectDisposedException)
            //         {
            //             connectionManager.RemoveConnection(connection);
            //             return;
            //         }
            //
            //         requestServiceProvider ??= serviceProvider;
            //
            //         HandleCollections(connection, contextName, dbContextType, changes, requestServiceProvider);
            //     });
            // });
        }

        private void HandleChangesOfCollection(Type dbContextType, IGrouping<string, ChangeResponse> collectionChanges,
            string contextName, List<ChangeResponse> changes)
        {
            KeyValuePair<Type, string> property = dbContextType.GetDbSetType(collectionChanges.Key);
            ModelAttributesInfo modelAttributesInfo = property.Key.GetModelAttributesInfo();
            Dictionary<string, List<CollectionSubscription>> equalCollectionSubscriptionsGrouping =
                subscriptionManager.GetSubscriptions(contextName, collectionChanges.Key);

            // TODO: Group subscriptions by prefilters directly and store prefilters only in Grouping instead of subscription

            if (equalCollectionSubscriptionsGrouping == null)
            {
                return;
            }

            Parallel.ForEach(equalCollectionSubscriptionsGrouping.Values, equalCollectionSubscriptions =>
            {
                if (!equalCollectionSubscriptions.Any())
                {
                    return;
                }

                Task.Run(() =>
                {
                    HandleEqualCollectionSubscriptions(equalCollectionSubscriptions, collectionChanges, dbContextType,
                        changes, property, modelAttributesInfo); 
                });

                // TODO: Test query function
                // TODO: Test authentication
            });
        }

        private void HandleEqualCollectionSubscriptions(List<CollectionSubscription> equalCollectionSubscriptions,
            IGrouping<string, ChangeResponse> collectionChanges, Type dbContextType, List<ChangeResponse> changes,
            KeyValuePair<Type, string> property, ModelAttributesInfo modelAttributesInfo)
        {
            CollectionSubscription firstCollectionSubscription = equalCollectionSubscriptions.FirstOrDefault();

            if (firstCollectionSubscription == null)
            {
                return;
            }

            List<IPrefilterBase> prefilters = firstCollectionSubscription.Prefilters;

            if (prefilters.Any(prefilter =>
                    prefilter is IAfterQueryPrefilter || prefilter is TakePrefilter ||
                    prefilter is SkipPrefilter)
                || HasIncludePrefilterWithChange(prefilters, collectionChanges.Key, changes))
            {
                // TODO: Get all subscriptions of same context with include prefilter

                HandleReloadOfCollectionData(dbContextType, property, prefilters, equalCollectionSubscriptions);
            }
            else
            {
                HandleRelativeChangesOfCollection(modelAttributesInfo, property, collectionChanges, prefilters,
                    equalCollectionSubscriptions);
            }
        }

        private void HandleRelativeChangesOfCollection(ModelAttributesInfo modelAttributesInfo,
            KeyValuePair<Type, string> property, IGrouping<string, ChangeResponse> collectionChanges,
            List<IPrefilterBase> prefilters, List<CollectionSubscription> equalCollectionSubscriptions)
        {
            List<ChangeResponse> completeChanges =
                CollectionChangeHelper.CalculateRelativeChangesWithQueryFunction(modelAttributesInfo,
                    property, collectionChanges.ToList(), serviceProvider);

            completeChanges = CollectionChangeHelper.CalculateRelativeChanges(prefilters, completeChanges,
                property);

            Parallel.ForEach(equalCollectionSubscriptions, subscription =>
            {
                // TODO: Check if use of requestServiceProvider is required

                Task.Run(() =>
                {
                    List<ChangeResponse> connectionChanges =
                        CollectionChangeHelper.CalculateRelativeAuthenticatedChanges(modelAttributesInfo,
                            completeChanges,
                            property, subscription.Connection.Information, serviceProvider);

                    ChangesResponse changesResponse = new ChangesResponse()
                    {
                        ReferenceId = subscription.ReferenceId,
                        Changes = connectionChanges.Select(change =>
                        {
                            object value =
                                change.Value.GetAuthenticatedQueryModel(subscription.Connection.Information,
                                    serviceProvider);
                            return change.CreateResponse(subscription.ReferenceId, value);
                        }).ToList()
                    };

                    if (changesResponse.Changes.Any())
                    {
                        _ = subscription.Connection.Send(changesResponse);
                    }
                });
            });
        }

        private void HandleReloadOfCollectionData(Type dbContextType, KeyValuePair<Type, string> property,
            List<IPrefilterBase> prefilters, List<CollectionSubscription> equalCollectionSubscriptions)
        {
            SapphireDbContext db = dbContextAccessor.GetContext(dbContextType, serviceProvider);

            IQueryable<object> collectionValues =
                db.GetCollectionValues(serviceProvider, property, prefilters);

            IAfterQueryPrefilter afterQueryPrefilter =
                prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

            if (afterQueryPrefilter != null)
            {
                afterQueryPrefilter.Initialize(property.Key);
                object result = afterQueryPrefilter.Execute(collectionValues);

                Parallel.ForEach(equalCollectionSubscriptions, subscription =>
                {
                    Task.Run(() =>
                    {
                        _ = subscription.Connection.Send(new QueryResponse()
                        {
                            ReferenceId = subscription.ReferenceId,
                            Result = result
                        });
                    });
                });
            }
            else
            {
                List<object> values = collectionValues.ToList();

                Parallel.ForEach(equalCollectionSubscriptions, subscription =>
                {
                    Task.Run(() =>
                    {
                        _ = subscription.Connection.Send(new QueryResponse()
                        {
                            ReferenceId = subscription.ReferenceId,
                            Result = values
                                .Where(v => property.Key.CanQueryEntry(subscription.Connection.Information,
                                    serviceProvider, v))
                                .Select(v =>
                                    v.GetAuthenticatedQueryModel(subscription.Connection.Information,
                                        serviceProvider))
                                .ToList()
                        });
                    });
                });
            }
        }

        private bool HasIncludePrefilterWithChange(List<IPrefilterBase> prefilters, string collectionName,
            List<ChangeResponse> allChanges)
        {
            List<IncludePrefilter> includePrefilters = prefilters.OfType<IncludePrefilter>().ToList();

            if (!includePrefilters.Any())
            {
                return false;
            }

            List<string> affectedCollections = includePrefilters
                .SelectMany(prefilter => prefilter.AffectedCollectionNames)
                .Distinct()
                .ToList();

            return allChanges.Any(change =>
                       change.CollectionName.Equals(collectionName,
                           StringComparison.InvariantCultureIgnoreCase)) ||
                   affectedCollections.Any(cName => allChanges.Any(change =>
                       change.CollectionName.Equals(cName, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}