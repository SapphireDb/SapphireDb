using System.Collections.Generic;

namespace SapphireDb.Models
{
    public class CollectionSubscriptionsContainer
    {
        public string CollectionName { get; set; }

        public IEnumerable<KeyValuePair<PrefilterContainer, List<CollectionSubscription>>> Subscriptions { get; set; }
    }
}