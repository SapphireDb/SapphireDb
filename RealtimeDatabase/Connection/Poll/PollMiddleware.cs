using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RealtimeDatabase.Connection.SSE;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Connection.Poll
{
    class PollMiddleware
    {
        private readonly RequestDelegate next;
        private readonly RealtimeConnectionManager connectionManager;
        private readonly RealtimeDatabaseOptions options;

        // ReSharper disable once UnusedParameter.Local
        public PollMiddleware(RequestDelegate next, RealtimeConnectionManager connectionManager, RealtimeDatabaseOptions options)
        {
            this.next = next;
            this.connectionManager = connectionManager;
            this.options = options;
        }

        public async Task Invoke(HttpContext context, ILogger<PollConnection> logger)
        {
            connectionManager.CheckExistingConnections();

            if (!AuthHelper.CheckApiAuth(context.Request.Headers["key"], context.Request.Headers["secret"], options))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(JsonHelper.Serialize(new WrongApiResponse()));
                return;
            }

            if (context.Request.Method != "GET")
            {
                await next(context);
                return;
            }

            PollConnection connection;

            if (context.Request.Path.Value.ToLowerInvariant().EndsWith("/init"))
            {
                connection = new PollConnection(context);

                connectionManager.AddConnection(connection);

                await context.Response.WriteAsync(JsonHelper.Serialize(new ConnectionResponse()
                {
                    ConnectionId = connection.Id,
                    BearerValid = context.User.Identity.IsAuthenticated
                }));

                logger.LogInformation("Created new poll connection");

            }
            else
            {
                connection = (PollConnection)connectionManager.GetConnection(context);

                if (connection != null)
                {
                    await context.Response.WriteAsync(JsonHelper.Serialize(connection.GetMessages()));
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("No connection was found.");
                }
            }
        }
    }
}
