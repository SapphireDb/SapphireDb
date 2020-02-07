using System;

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

        public Exception Error { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
