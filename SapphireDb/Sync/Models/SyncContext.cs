using System;

namespace SapphireDb.Sync.Models
{
    public class SyncContext
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();
    }
}