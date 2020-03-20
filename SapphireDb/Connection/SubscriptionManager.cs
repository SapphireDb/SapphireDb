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
        private readonly Dictionary<string, Dictionary<string, Dictionary<PrefilterContainer, List<CollectionSubscription>>>> subscriptions =
            new Dictionary<string, Dictionary<string, Dictionary<PrefilterContainer, List<CollectionSubscription>>>>(StringComparer.InvariantCultureIgnoreCase);

        public void AddSubscription(string contextName, string collectionName, List<IPrefilterBase> prefilters,
            ConnectionBase connection, string referenceId)
        {
            contextName = contextName.ToLowerInvariant();
            collectionName = collectionName.ToLowerInvariant();

            try
            {
                readerWriterLockSlim.EnterWriteLock();

                if (!subscriptions.TryGetValue(contextName,
                    out Dictionary<string, Dictionary<PrefilterContainer, List<CollectionSubscription>>>
                        subscriptionsOfContext))
                {
                    subscriptionsOfContext =
                        new Dictionary<string, Dictionary<PrefilterContainer, List<CollectionSubscription>>>(
                            StringComparer.InvariantCultureIgnoreCase);
                    subscriptions.Add(contextName, subscriptionsOfContext);
                }

                if (!subscriptionsOfContext.TryGetValue(collectionName,
                    out Dictionary<PrefilterContainer, List<CollectionSubscription>> subscriptionsOfCollection))
                {
                    subscriptionsOfCollection = new Dictionary<PrefilterContainer, List<CollectionSubscription>>();
                    subscriptionsOfContext.Add(collectionName, subscriptionsOfCollection);
                }

                PrefilterContainer prefilterContainer = new PrefilterContainer(prefilters);

                if (!subscriptionsOfCollection.TryGetValue(prefilterContainer,
                    out List<CollectionSubscription> equalCollectionSubscriptions))
                {
                    equalCollectionSubscriptions = new List<CollectionSubscription>();
                    subscriptionsOfCollection.Add(prefilterContainer, equalCollectionSubscriptions);
                }

                CollectionSubscription subscription = new CollectionSubscription()
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

        public void RemoveConnectionSubscriptions(Guid connectionId)
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

        public Dictionary<PrefilterContainer, List<CollectionSubscription>> GetSubscriptions(string contextName, string collectionName)
        {
            try
            {
                readerWriterLockSlim.EnterReadLock();
            
                if (subscriptions.TryGetValue(contextName,
                    out Dictionary<string, Dictionary<PrefilterContainer, List<CollectionSubscription>>> subscriptionsOfContext))
                {
                    if (subscriptionsOfContext.TryGetValue(collectionName,
                        out Dictionary<PrefilterContainer, List<CollectionSubscription>> subscriptionsOfCollection))
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
        
        private void CleanupSubscriptionGrouping(KeyValuePair<PrefilterContainer, List<CollectionSubscription>> equalSubscriptions,
            KeyValuePair<string, Dictionary<PrefilterContainer, List<CollectionSubscription>>> collectionSubscriptions,
            KeyValuePair<string, Dictionary<string, Dictionary<PrefilterContainer, List<CollectionSubscription>>>> contextSubscriptions)
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