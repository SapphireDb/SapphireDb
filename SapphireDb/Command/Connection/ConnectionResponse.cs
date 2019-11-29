using System;

namespace SapphireDb.Command.Connection
{
    class ConnectionResponse : ResponseBase
    {
        public Guid ConnectionId { get; set; }

        public bool BearerValid { get; set; }
    }
}
