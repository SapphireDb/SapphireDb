using System;
using System.Collections.Generic;
using System.Text;

namespace SapphireDb.Sync.Models
{
    class SendMessageRequest : SyncRequest
    {
        public object Message { get; set; }
        
        public string Filter { get; set; }
        
        public object[] FilterParameters { get; set; }
    }
}
