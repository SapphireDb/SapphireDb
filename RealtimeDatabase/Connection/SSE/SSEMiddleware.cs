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
            SSEConnection connection = null;

            try
            {
                if (context.Request.Headers["Accept"] == "text/event-stream")
                {
                    context.Response.Headers["Cache-Control"] = "no-cache";
                    context.Response.Headers["X-Accel-Buffering"] = "no";
                    context.Response.ContentType = "text/event-stream";
                    context.Response.Body.Flush();

                    connection = new SSEConnection(context);

                    if (!AuthHelper.CheckApiAuth(context.Request.Query["key"], context.Request.Query["secret"], options))
                    {
                        await connection.Send(new WrongApiResponse());
                        context.Response.Body.Close();
                        return;
                    }

                    connectionManager.AddConnection(connection);
                    await connection.Send(new ConnectionResponse()
                    {
                        ConnectionId = connection.Id,
                        BearerValid = context.User.Identity.IsAuthenticated
                    });

                    context.RequestAborted.WaitHandle.WaitOne();
                }
            }
            catch (Exception ex)
            {
                await context.Response.WriteAsync(ex.Message);
            }
            finally
            {
                if (connection != null)
                {
                    connectionManager.RemoveConnection(connection);
                }
            }
            
        }
    }
}
