using System;

namespace RealtimeDatabase.Models.Responses
{
    public class ResponseBase
    {
        public string ResponseType {
            get {
                return GetType().Name;
            }
        }

        public string ReferenceId { get; set; }

        public Exception Error { get; set; }
    }
}
