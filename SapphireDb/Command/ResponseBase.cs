using System;
using SapphireDb.Models;

namespace SapphireDb.Command
{
    public class ResponseBase
    {
        public ResponseBase()
        {
            Timestamp = DateTime.UtcNow;
        }
        
        public string ResponseType => GetType().Name;

        public string ReferenceId { get; set; }

        public SapphireDbErrorResponse Error { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
