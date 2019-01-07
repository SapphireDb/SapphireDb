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
        private readonly RealtimeDatabaseOptions options;

        public RealtimeDatabaseWebsocketMiddleware(RequestDelegate _next, WebsocketConnectionManager _connectionManager, RealtimeDatabaseOptions _options)
        {
            next = _next;
            connectionManager = _connectionManager;
            options = _options;
        }

        public async Task Invoke(HttpContext context, WebsocketCommandHandler commandHandler, ILogger<WebsocketConnection> logger)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                if (!String.IsNullOrEmpty(options.Secret))
                {
                    if (context.Request.Query["secret"] != options.Secret)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("The secret does not match");
                        return;
                    }
                }

                if (options.Authentication == RealtimeDatabaseOptions.AuthenticationMode.Always)
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("The user is not authenticated");
                        return;
                    }
                }

                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                WebsocketConnection connection = new WebsocketConnection(webSocket, context);

                connectionManager.AddConnection(connection);

                while (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.Connecting)
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
