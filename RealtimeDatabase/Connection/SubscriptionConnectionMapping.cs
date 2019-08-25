using RealtimeDatabase.Models;
using System.Threading;

namespace RealtimeDatabase.Connection
{
    class SubscriptionConnectionMapping
    {
        public ConnectionBase Connection { get; set; }

        public CollectionSubscription Subscription { get; set; } 
    }
}
