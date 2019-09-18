using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class ConnectionResponse : ResponseBase
    {
        public Guid ConnectionId { get; set; }

        public bool BearerValid { get; set; }
    }
}
