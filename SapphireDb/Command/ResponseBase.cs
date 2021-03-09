using System;

namespace SapphireDb.Command
{
    public class ResponseBase
    {
        public ResponseBase()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }
        
        public string ResponseType => GetType().Name;

        public string ReferenceId { get; set; }

        public SapphireDbErrorResponse Error { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
