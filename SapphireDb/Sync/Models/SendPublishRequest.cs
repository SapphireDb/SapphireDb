namespace SapphireDb.Sync.Models
{
    public class SendPublishRequest : SyncRequest
    {
        public string Topic { get; set; }

        public object Message { get; set; }
        
        public bool Retain { get; set; }
    }
}
