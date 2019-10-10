using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models.Commands;

namespace RealtimeDatabase.Connection.SSE
{
    class SSEMiddleware
    {
        private readonly RealtimeConnectionManager connectionManager;
        private readonly RealtimeDatabaseOptions options;

        // ReSharper disable once UnusedParameter.Local
        public SSEMiddleware(RequestDelegate next, RealtimeConnectionManager connectionManager, RealtimeDatabaseOptions options)
        {
            this.connectionManager = connectionManager;
            this.options = options;
        }

        public async Task Invoke(HttpContext context, CommandExecutor commandExecutor, IServiceProvider serviceProvider, ILogger<SSEConnection> logger)
        {
            if (context.Request.Headers["Accept"] == "text/event-stream" && await CheckAuthentication(context))
            {
                context.Response.Headers["Cache-Control"] = "no-cache";
                context.Response.Headers["X-Accel-Buffering"] = "no";
                context.Response.ContentType = "text/event-stream";
                context.Response.Body.Flush();

                SSEConnection connection = new SSEConnection(context);

                connectionManager.AddConnection(connection);
                await connection.Send(new ConnectionResponse() {
                    ConnectionId = connection.Id,
                    BearerValid = context.User.Identity.IsAuthenticated
                });

                context.RequestAborted.WaitHandle.WaitOne();
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

            return true;
        }
    }
}
