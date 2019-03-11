using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class QueryConnectionsResponse : ResponseBase
    {
        public List<WebsocketConnection> Connections { get; set; }
    }
}
