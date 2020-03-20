using SapphireDb.Connection;

namespace SapphireDb.Models
{
    public class Subscription
    {
        public string ReferenceId { get; set; }
        
        public ConnectionBase Connection { get; set; }
    }
}
