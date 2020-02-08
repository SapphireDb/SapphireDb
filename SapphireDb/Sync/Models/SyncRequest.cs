namespace SapphireDb.Sync.Models
{
    public class SyncRequest
    {
        public bool Propagate { get; set; }

        public string OriginId { get; set; }
    }
}