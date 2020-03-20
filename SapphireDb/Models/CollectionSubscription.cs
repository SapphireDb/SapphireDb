using SapphireDb.Connection;

namespace SapphireDb.Models
{
    public class CollectionSubscription
    {
        public string ReferenceId { get; set; }
        
        public ConnectionBase Connection { get; set; }
    }
}
