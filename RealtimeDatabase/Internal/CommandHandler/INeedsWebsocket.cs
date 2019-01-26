using System;
using System.Collections.Generic;
using System.Text;
using RealtimeDatabase.Websocket.Models;

namespace RealtimeDatabase.Internal.CommandHandler
{
    interface INeedsWebsocket
    {
        void InsertWebsocket(WebsocketConnection currentWebsocketConnection);
    }
}
