using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RealtimeDatabase.Models;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    class RealtimeDatabaseWebsocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly WebsocketConnectionManager connectionManager;

        public RealtimeDatabaseWebsocketMiddleware(RequestDelegate _next, WebsocketConnectionManager _connectionManager)
        {
            next = _next;
            connectionManager = _connectionManager;
        }

        public async Task Invoke(HttpContext context, WebsocketCommandHandler commandHandler, ILogger<WebsocketConnection> logger)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                WebsocketConnection connection = new WebsocketConnection(webSocket, context);

                connectionManager.AddConnection(connection);

                while (!webSocket.CloseStatus.HasValue)
                {
                    try
                    {
                        await commandHandler.HandleCommand(connection);
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
                }

                connectionManager.RemoveConnection(connection);
            }
        }
    }
}
