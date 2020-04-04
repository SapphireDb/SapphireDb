using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class MessageSubscriptionManager
    {
        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private readonly Dictionary<string, List<Subscription>> subscriptions =
            new Dictionary<string, List<Subscription>>(StringComparer.InvariantCultureIgnoreCase);

        public void AddSubscription(string topic, string referenceId, ConnectionBase connection)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();

                topic = topic.ToLowerInvariant();

                if (!subscriptions.TryGetValue(topic, out List<Subscription> topicSubscriptions))
                {
                    topicSubscriptions = new List<Subscription>();
                    subscriptions.Add(topic, topicSubscriptions);
                }

                topicSubscriptions.Add(new Subscription()
                {
                    Connection = connection,
                    ReferenceId = referenceId
                });
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

                foreach (var topicSubscriptions in subscriptions)
                {
                    topicSubscriptions.Value.RemoveAll(s => s.ReferenceId == referenceId);
                    CleanupSubscriptions(topicSubscriptions);
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

                foreach (var topicSubscriptions in subscriptions)
                {
                    topicSubscriptions.Value.RemoveAll(s => s.Connection.Id == connectionId);
                    CleanupSubscriptions(topicSubscriptions);
                }
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public List<Subscription> GetTopicSubscriptions(string topic)
        {
            try
            {
                readerWriterLockSlim.EnterReadLock();

                return subscriptions
                    .Where(s => topic.MatchesGlobPattern(s.Key))
                    .SelectMany(s => s.Value)
                    .ToList();
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }
        
        private void CleanupSubscriptions(KeyValuePair<string, List<Subscription>> topicSubscriptions)
        {
            if (!topicSubscriptions.Value.Any())
            {
                subscriptions.Remove(topicSubscriptions.Key);
            }
        }
    }
}