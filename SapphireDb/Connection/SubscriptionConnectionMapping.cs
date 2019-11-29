using SapphireDb.Models;
using System.Threading;

namespace SapphireDb.Connection
{
    class SubscriptionConnectionMapping
    {
        public ConnectionBase Connection { get; set; }

        public CollectionSubscription Subscription { get; set; } 
    }
}
