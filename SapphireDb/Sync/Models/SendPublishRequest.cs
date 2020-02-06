using System;
using System.Collections.Generic;
using System.Text;

namespace SapphireDb.Sync.Models
{
    class SendPublishRequest
    {
        public string Topic { get; set; }

        public object Message { get; set; }
        
        public bool Retain { get; set; }
    }
}
