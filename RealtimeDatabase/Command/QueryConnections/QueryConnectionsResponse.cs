using System.Collections.Generic;
using RealtimeDatabase.Connection;

namespace RealtimeDatabase.Command.QueryConnections
{
    class QueryConnectionsResponse : ResponseBase
    {
        public List<ConnectionBase> Connections { get; set; }
    }
}
