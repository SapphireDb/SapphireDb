using System;
using System.Collections.Generic;
using System.Text;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Connection.Websocket;

namespace RealtimeDatabase.Models.Responses
{
    class QueryConnectionsResponse : ResponseBase
    {
        public List<ConnectionBase> Connections { get; set; }
    }
}
