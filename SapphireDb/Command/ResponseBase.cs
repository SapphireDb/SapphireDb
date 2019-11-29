using System;

namespace SapphireDb.Command
{
    public class ResponseBase
    {
        public string ResponseType => GetType().Name;

        public string ReferenceId { get; set; }

        public Exception Error { get; set; }
    }
}
