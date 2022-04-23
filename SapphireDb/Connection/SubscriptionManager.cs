using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class SubscriptionManager
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private readonly Dictionary<string, Dictionary<string, Dictionary<PrefilterContainer, List<Subscription>>>> subscriptions =
            new Dictionary<string, Dictionary<string, Dictionary<PrefilterContainer, List<Subscription>>>>(StringComparer.InvariantCultureIgnoreCase);

        public void AddSubscription(string contextName, string collectionName, List<IPrefilterBase> prefilters,
            SignalRConnection connection, string referenceId)
        {
            contextName = contextName.ToLowerInvariant();
            collectionName = collectionName.ToLowerInvariant();

            try
            {
                readerWriterLockSlim.EnterWriteLock();

                if (!subscriptions.TryGetValue(contextName,
                    out Dictionary<string, Dictionary<PrefilterContainer, List<Subscription>>>
                        subscriptionsOfContext))
                {
                    subscriptionsOfContext =
                        new Dictionary<string, Dictionary<PrefilterContainer, List<Subscription>>>(
                            StringComparer.InvariantCultureIgnoreCase);
                    subscriptions.Add(contextName, subscriptionsOfContext);
                }

                if (!subscriptionsOfContext.TryGetValue(collectionName,
                    out Dictionary<PrefilterContainer, List<Subscription>> subscriptionsOfCollection))
                {
                    subscriptionsOfCollection = new Dictionary<PrefilterContainer, List<Subscription>>();
                    subscriptionsOfContext.Add(collectionName, subscriptionsOfCollection);
                }

                PrefilterContainer prefilterContainer = new PrefilterContainer(prefilters);

                if (!subscriptionsOfCollection.TryGetValue(prefilterContainer,
                    out List<Subscription> equalCollectionSubscriptions))
                {
                    equalCollectionSubscriptions = new List<Subscription>();
                    subscriptionsOfCollection.Add(prefilterContainer, equalCollectionSubscriptions);
                }

                Subscription subscription = new Subscription()
                {
                    Connection = connection,
                    ReferenceId = referenceId
                };

                equalCollectionSubscriptions.Add(subscription);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void RemoveSubscription(string referenceId)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();

                foreach (var contextSubscriptions in subscriptions)
                {
                    foreach (var collectionSubscriptions in contextSubscriptions.Value)
                    {
                        foreach (var equalSubscriptions in collectionSubscriptions.Value)
                        {
                            equalSubscriptions.Value.RemoveAll(subscription => subscription.ReferenceId == referenceId);
                            CleanupSubscriptionGrouping(equalSubscriptions, collectionSubscriptions,
                                contextSubscriptions);
                        }
                    }
                }
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void RemoveConnectionSubscriptions(string connectionId)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();

                foreach (var contextSubscriptions in subscriptions)
                {
                    foreach (var collectionSubscriptions in contextSubscriptions.Value)
                    {
                        foreach (var equalSubscriptions in collectionSubscriptions.Value)
                        {
                            equalSubscriptions.Value.RemoveAll(subscription =>
                                subscription.Connection.Id == connectionId);
                            CleanupSubscriptionGrouping(equalSubscriptions, collectionSubscriptions,
                                contextSubscriptions);
                        }
                    }
                }
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public Dictionary<PrefilterContainer, List<Subscription>> GetSubscriptions(string contextName, string collectionName)
        {
            try
            {
                readerWriterLockSlim.EnterReadLock();
            
                if (subscriptions.TryGetValue(contextName,
                    out Dictionary<string, Dictionary<PrefilterContainer, List<Subscription>>> subscriptionsOfContext))
                {
                    if (subscriptionsOfContext.TryGetValue(collectionName,
                        out Dictionary<PrefilterContainer, List<Subscription>> subscriptionsOfCollection))
                    {
                        return subscriptionsOfCollection;
                    }
                }
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }

            return null;
        }

        public List<CollectionSubscriptionsContainer> GetSubscriptionsWithInclude(string contextName, string collectionName)
        {
            try
            {
                readerWriterLockSlim.EnterReadLock();

                if (!subscriptions.TryGetValue(contextName, out var contextSubscriptions))
                {
                    return null;
                }

                var includeSubscriptions = contextSubscriptions
                    .Select(collectionSubscriptions =>
                    {
                        return new CollectionSubscriptionsContainer()
                        {
                            CollectionName = collectionSubscriptions.Key,
                            Subscriptions = collectionSubscriptions.Value
                                .Where(equalSubscriptions => equalSubscriptions.Key.HasAffectedIncludePrefilter(collectionName))
                        };
                    })
                    .Where(container => container.Subscriptions.Any())
                    .ToList();
                
                return includeSubscriptions;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }
        
        private void CleanupSubscriptionGrouping(KeyValuePair<PrefilterContainer, List<Subscription>> equalSubscriptions,
            KeyValuePair<string, Dictionary<PrefilterContainer, List<Subscription>>> collectionSubscriptions,
            KeyValuePair<string, Dictionary<string, Dictionary<PrefilterContainer, List<Subscription>>>> contextSubscriptions)
        {
            if (!equalSubscriptions.Value.Any())
            {
                collectionSubscriptions.Value.Remove(equalSubscriptions.Key);
                equalSubscriptions.Key.Dispose();

                if (!collectionSubscriptions.Value.Keys.Any())
                {
                    contextSubscriptions.Value.Remove(collectionSubscriptions.Key);

                    if (!contextSubscriptions.Value.Keys.Any())
                    {
                        subscriptions.Remove(contextSubscriptions.Key);
                    }
                }
            }
        }
    }
}