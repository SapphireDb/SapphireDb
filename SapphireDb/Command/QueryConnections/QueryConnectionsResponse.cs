using System.Collections.Generic;
using SapphireDb.Connection;

namespace SapphireDb.Command.QueryConnections
{
    class QueryConnectionsResponse : ResponseBase
    {
        public List<ConnectionBase> Connections { get; set; }
    }
}
