using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Primitives;
using RealtimeDatabase.Connection;

namespace RealtimeDatabase.Connection.Websocket
{
    public class WebsocketConnection : ConnectionBase, IDisposable
    {
        public WebsocketConnection(WebSocket webSocket, HttpContext context)
        {
            Websocket = webSocket;
            Init(context);
        }

        public WebSocket Websocket { get; set; }

        public override string Type => "Websocket";

        public override async Task Send(object message)
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
            return Websocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "The connection was closed by another client", WebsocketHelper.token);
        }

        public new void Dispose()
        {
            base.Dispose();
            Websocket.Dispose();
        }
    }
}
