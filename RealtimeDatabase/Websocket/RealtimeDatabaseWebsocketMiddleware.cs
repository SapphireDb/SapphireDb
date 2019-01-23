using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RealtimeDatabase.Models;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    class RealtimeDatabaseWebsocketMiddleware
    {
        private readonly WebsocketConnectionManager connectionManager;
        private readonly RealtimeDatabaseOptions options;

        // ReSharper disable once UnusedParameter.Local
        public RealtimeDatabaseWebsocketMiddleware(RequestDelegate next, WebsocketConnectionManager connectionManager, RealtimeDatabaseOptions options)
        {
            this.connectionManager = connectionManager;
            this.options = options;
        }

        public async Task Invoke(HttpContext context, WebsocketCommandHandler commandHandler, ILogger<WebsocketConnection> logger)
        {
            if (context.WebSockets.IsWebSocketRequest && await CheckAuthentication(context))
            {
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

        private async Task<bool> CheckAuthentication(HttpContext context)
        {
            if (!string.IsNullOrEmpty(options.Secret))
            {
                if (context.Request.Query["secret"] != options.Secret)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("The secret does not match");
                    return false;
                }
            }

            if (options.Authentication == RealtimeDatabaseOptions.AuthenticationMode.Always)
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("The user is not authenticated");
                    return false;
                }
            }

            return true;
        }
    }
}
