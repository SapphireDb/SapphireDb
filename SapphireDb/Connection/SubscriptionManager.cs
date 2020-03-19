using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class SubscriptionManager
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, List<CollectionSubscription>>>> subscriptions =
            new Dictionary<string, Dictionary<string, Dictionary<string, List<CollectionSubscription>>>>(StringComparer.InvariantCultureIgnoreCase);

        public void AddSubscription(CollectionSubscription subscription)
        {
            readerWriterLockSlim.EnterWriteLock();

            if (!subscriptions.TryGetValue(subscription.ContextName, out Dictionary<string, Dictionary<string, List<CollectionSubscription>>> subscriptionsOfContext))
            {
                subscriptionsOfContext = new Dictionary<string, Dictionary<string, List<CollectionSubscription>>>(StringComparer.InvariantCultureIgnoreCase);
                subscriptions.Add(subscription.ContextName.ToLowerInvariant(), subscriptionsOfContext);
            }

            if (!subscriptionsOfContext.TryGetValue(subscription.CollectionName,
                out Dictionary<string, List<CollectionSubscription>> subscriptionsOfCollection))
            {
                subscriptionsOfCollection = new Dictionary<string, List<CollectionSubscription>>();
                subscriptionsOfContext.Add(subscription.CollectionName.ToLowerInvariant(), subscriptionsOfCollection);
            }
            
            if (!subscriptionsOfCollection.TryGetValue(subscription.PrefilterHash,
                out List<CollectionSubscription> equalCollectionSubscriptions))
            {
                equalCollectionSubscriptions = new List<CollectionSubscription>();
                subscriptionsOfCollection.Add(subscription.PrefilterHash, equalCollectionSubscriptions);
            }

            equalCollectionSubscriptions.Add(subscription);

            readerWriterLockSlim.ExitWriteLock();
        }

        public void RemoveSubscription(string referenceId)
        {
            readerWriterLockSlim.EnterWriteLock();
            
            foreach (var contextSubscriptions in subscriptions)
            {
                foreach (var collectionSubscriptions in contextSubscriptions.Value)
                {
                    foreach (var equalSubscriptions in collectionSubscriptions.Value)
                    {
                        equalSubscriptions.Value.RemoveAll(subscription => subscription.ReferenceId == referenceId);
                    }
                }
            }
            
            readerWriterLockSlim.ExitWriteLock();
        }

        public void RemoveConnectionSubscriptions(Guid connectionId)
        {
            readerWriterLockSlim.EnterWriteLock();
            
            foreach (var contextSubscriptions in subscriptions)
            {
                foreach (var collectionSubscriptions in contextSubscriptions.Value)
                {
                    foreach (var equalSubscriptions in collectionSubscriptions.Value)
                    {
                        equalSubscriptions.Value.RemoveAll(subscription => subscription.Connection.Id == connectionId);
                    }
                }
            }
            
            readerWriterLockSlim.ExitWriteLock();
        }
        
        public Dictionary<string, List<CollectionSubscription>> GetSubscriptions(string contextName, string collectionName)
        {
            readerWriterLockSlim.EnterReadLock();
            
            if (subscriptions.TryGetValue(contextName,
                out Dictionary<string, Dictionary<string, List<CollectionSubscription>>> subscriptionsOfContext))
            {
                if (subscriptionsOfContext.TryGetValue(collectionName,
                    out Dictionary<string, List<CollectionSubscription>> subscriptionsOfCollection))
                {
                    readerWriterLockSlim.ExitReadLock();
                    return subscriptionsOfCollection;
                }
            }
            
            readerWriterLockSlim.ExitReadLock();
            return null;
        }
    }
}