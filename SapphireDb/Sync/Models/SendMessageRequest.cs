namespace SapphireDb.Sync.Models
{
    public class SendMessageRequest : SyncRequest
    {
        public object Message { get; set; }
        
        public string Filter { get; set; }
        
        public object[] FilterParameters { get; set; }
    }
}
