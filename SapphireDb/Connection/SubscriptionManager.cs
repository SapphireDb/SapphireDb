using System;
using System.Collections.Concurrent;

namespace SapphireDb.Connection
{
    public class SubscriptionManager
    {
        private ConcurrentDictionary<SubscriptionGroupingKey, string> subscriptions = new ConcurrentDictionary<SubscriptionGroupingKey, string>();
    }

    class SubscriptionGroupingKey
    {
        public Type DbContextType { get; set; }

        public string CollectionName { get; set; }

        public string PrefilterHash { get; set; }
    }
}