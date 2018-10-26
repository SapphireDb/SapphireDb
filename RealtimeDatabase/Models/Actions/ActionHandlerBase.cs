using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Actions
{
    public class ActionHandlerBase
    {
        public WebsocketConnection WebsocketConnection { get; set; }
    }
}
