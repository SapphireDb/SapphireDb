using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Command.Query;
using SapphireDb.Command.Subscribe;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class SapphireChangeNotifier
    {
        private readonly DbContextAccesor _dbContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextTypeContainer _contextTypeContainer;
        private readonly SubscriptionManager _subscriptionManager;

        public SapphireChangeNotifier(
            DbContextAccesor dbContextAccessor,
            IServiceProvider serviceProvider,
            DbContextTypeContainer contextTypeContainer,
            SubscriptionManager subscriptionManager)
        {
            _dbContextAccessor = dbContextAccessor;
            _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            _contextTypeContainer = contextTypeContainer;
            _subscriptionManager = subscriptionManager;
        }

        public void HandleChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            Guid handlingId = Guid.NewGuid();
            string contextName = _contextTypeContainer.GetName(dbContextType);

            Parallel.ForEach(changes.GroupBy(change => change.CollectionName),
                collectionChanges =>
                {
                    Task.Run(() =>
                    {
                        HandleChangesOfCollection(dbContextType, collectionChanges, contextName, changes,
                            handlingId);
                    });

                    Task.Run(() =>
                    {
                        HandleSubscriptionsWithAffectedInclude(dbContextType, contextName, collectionChanges,
                            handlingId);
                    });
                });
        }

        private void HandleChangesOfCollection(Type dbContextType, IGrouping<string, ChangeResponse> collectionChanges,
            string contextName, List<ChangeResponse> changes, Guid handlingId)
        {
            KeyValuePair<Type, string> property = dbContextType.GetDbSetType(collectionChanges.Key);
            ModelAttributesInfo modelAttributesInfo = property.Key.GetModelAttributesInfo();
            Dictionary<PrefilterContainer, List<Subscription>> equalCollectionSubscriptionsGrouping =
                _subscriptionManager.GetSubscriptions(contextName, collectionChanges.Key);

            if (equalCollectionSubscriptionsGrouping == null)
            {
                return;
            }

            Parallel.ForEach(equalCollectionSubscriptionsGrouping, equalCollectionSubscriptions =>
            {
                Task.Run(() =>
                {
                    HandleEqualCollectionSubscriptions(equalCollectionSubscriptions.Key,
                        equalCollectionSubscriptions.Value, collectionChanges, dbContextType, changes, property,
                        modelAttributesInfo, handlingId);
                });
            });
        }

        private void HandleSubscriptionsWithAffectedInclude(Type dbContextType, string contextName,
            IGrouping<string, ChangeResponse> collectionChanges, Guid handlingId)
        {
            List<CollectionSubscriptionsContainer> equalCollectionSubscriptions =
                _subscriptionManager.GetSubscriptionsWithInclude(contextName, collectionChanges.Key);

            if (equalCollectionSubscriptions == null)
            {
                return;
            }

            Parallel.ForEach(equalCollectionSubscriptions, collectionSubscriptions =>
            {
                Task.Run(() =>
                {
                    KeyValuePair<Type, string> property =
                        dbContextType.GetDbSetType(collectionSubscriptions.CollectionName);

                    Parallel.ForEach(collectionSubscriptions.Subscriptions, subscriptionGroupings =>
                    {
                        Task.Run(() =>
                        {
                            HandleReloadOfCollectionData(dbContextType, property, subscriptionGroupings.Key,
                                subscriptionGroupings.Value, handlingId);
                        });
                    });
                });
            });
        }

        private void HandleEqualCollectionSubscriptions(PrefilterContainer prefilterContainer,
            List<Subscription> equalCollectionSubscriptions,
            IGrouping<string, ChangeResponse> collectionChanges, Type dbContextType, List<ChangeResponse> changes,
            KeyValuePair<Type, string> property, ModelAttributesInfo modelAttributesInfo, Guid handlingId)
        {
            List<IPrefilterBase> prefilters = prefilterContainer.Prefilters;

            if (prefilters.Any(prefilter =>
                    prefilter is IAfterQueryPrefilter || prefilter is TakePrefilter ||
                    prefilter is SkipPrefilter || prefilter is IncludePrefilter))
            {
                HandleReloadOfCollectionData(dbContextType, property, prefilterContainer, equalCollectionSubscriptions,
                    handlingId);
            }
            else
            {
                HandleRelativeChangesOfCollection(modelAttributesInfo, property, collectionChanges, prefilters,
                    equalCollectionSubscriptions);
            }
        }

        private void HandleRelativeChangesOfCollection(ModelAttributesInfo modelAttributesInfo,
            KeyValuePair<Type, string> property, IGrouping<string, ChangeResponse> collectionChanges,
            List<IPrefilterBase> prefilters, List<Subscription> equalCollectionSubscriptions)
        {
            List<ChangeResponse> completeChanges = CollectionChangeHelper.CalculateRelativeChanges(prefilters,
                collectionChanges.ToList(), property);

            Parallel.ForEach(equalCollectionSubscriptions, subscription =>
            {
                Task.Run(() =>
                {
                    List<ChangeResponse> connectionChanges =
                        CollectionChangeHelper.CalculateRelativeAuthenticatedChanges(modelAttributesInfo,
                            completeChanges,
                            property, subscription.Connection, _serviceProvider);

                    ChangesResponse changesResponse = new ChangesResponse()
                    {
                        ReferenceId = subscription.ReferenceId,
                        Changes = connectionChanges.Select(change =>
                        {
                            object value =
                                change.Value.GetAuthenticatedQueryModel(subscription.Connection,
                                    _serviceProvider);
                            return change.CreateResponse(subscription.ReferenceId, value);
                        }).ToList()
                    };

                    if (changesResponse.Changes.Any())
                    {
                        _ = subscription.Connection.Send(changesResponse, _serviceProvider);
                    }
                });
            });
        }

        private void HandleReloadOfCollectionData(Type dbContextType, KeyValuePair<Type, string> property,
            PrefilterContainer prefilterContainer,
            List<Subscription> equalCollectionSubscriptions, Guid handlingId)
        {
            try
            {
                if (!prefilterContainer.StartHandling(handlingId))
                {
                    return;
                }

                DbContext db = _dbContextAccessor.GetContext(dbContextType, _serviceProvider);

                IQueryable<object> collectionValues =
                    db.GetCollectionValues(property, prefilterContainer.Prefilters);

                IAfterQueryPrefilter afterQueryPrefilter =
                    prefilterContainer.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

                if (afterQueryPrefilter != null)
                {
                    object result = afterQueryPrefilter.Execute(collectionValues);

                    Parallel.ForEach(equalCollectionSubscriptions, subscription =>
                    {
                        Task.Run(() =>
                        {
                            _ = subscription.Connection.Send(new QueryResponse()
                            {
                                ReferenceId = subscription.ReferenceId,
                                Result = result
                            }, _serviceProvider);
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
                                    .Where(v => property.Key.CanQueryEntry(subscription.Connection,
                                        _serviceProvider, v))
                                    .Select(v =>
                                        v.GetAuthenticatedQueryModel(subscription.Connection,
                                            _serviceProvider))
                                    .ToList()
                            }, _serviceProvider);
                        });
                    });
                }
            }
            finally
            {
                prefilterContainer.FinishHandling();
            }
        }
    }
}