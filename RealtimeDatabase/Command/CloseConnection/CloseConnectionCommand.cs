using System;

namespace RealtimeDatabase.Command.CloseConnection
{
    class CloseConnectionCommand : CommandBase
    {
        public Guid ConnectionId { get; set; }

        public bool DeleteRenewToken { get; set; }
    }
}
