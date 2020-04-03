using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Command;

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
