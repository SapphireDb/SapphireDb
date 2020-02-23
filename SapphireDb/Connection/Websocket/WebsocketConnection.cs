using Microsoft.AspNetCore.Http;
using SapphireDb.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Primitives;
using SapphireDb.Command;
using SapphireDb.Connection;

namespace SapphireDb.Connection.Websocket
{
    public class WebsocketConnection : ConnectionBase, IDisposable
    {
        public WebsocketConnection(WebSocket webSocket, HttpContext context)
        {
            Init(context);
            Websocket = webSocket;
            HttpContext = null;
        }

        public WebSocket Websocket { get; set; }

        public override string Type => "Websocket";

        public override async Task Send(ResponseBase message)
        {
            await Lock.WaitAsync();

            try
            {
                await Websocket.Send(message);
            }
            finally
            {
                Lock.Release();
            }
        }

        public override Task Close()
        {
            return Websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "The connection was closed by another client", WebsocketHelper.token);
        }

        public new void Dispose()
        {
            base.Dispose();
            Websocket.Dispose();
        }
    }
}
