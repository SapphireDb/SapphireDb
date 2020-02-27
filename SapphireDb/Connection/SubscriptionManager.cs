using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class SubscriptionManager
    {
        private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private ConcurrentDictionary<SubscriptionGroupingKey, List<CollectionSubscription>> subscriptions =
            new ConcurrentDictionary<SubscriptionGroupingKey, List<CollectionSubscription>>();

        public void AddSubscription(CollectionSubscription subscription)
        {
            readerWriterLockSlim.EnterWriteLock();

            // Add to dictionary (or create lookup?)
            
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    class SubscriptionGroupingKey : IEquatable<SubscriptionGroupingKey>
    {
        public Type DbContextType { get; set; }

        public string CollectionName { get; set; }

        public bool RequiresDatabaseQuery { get; set; }
        // public string PrefilterHash { get; set; }

        public bool Equals(SubscriptionGroupingKey other)
        {
            return DbContextType == other?.DbContextType && CollectionName == other?.CollectionName &&
                   RequiresDatabaseQuery == other?.RequiresDatabaseQuery;
        }
    }
}