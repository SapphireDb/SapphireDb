using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SapphireDb.Command.Connection;
using SapphireDb.Connection.SSE;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Connection.Poll
{
    class PollMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ConnectionManager connectionManager;
        private readonly SapphireDatabaseOptions options;

        // ReSharper disable once UnusedParameter.Local
        public PollMiddleware(RequestDelegate next, ConnectionManager connectionManager, SapphireDatabaseOptions options)
        {
            this.next = next;
            this.connectionManager = connectionManager;
            this.options = options;
        }

        public async Task Invoke(HttpContext context, ILogger<PollConnection> logger)
        {
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
                    ConnectionId = connection.Id
                }));

                logger.LogInformation("Created new poll connection");

            }
            else
            {
                connection = (PollConnection)connectionManager.GetConnection(context);

                if (connection != null)
                {
                    IEnumerable<object> messages =  await connection.GetMessages(context.RequestAborted);
                    await context.Response.WriteAsync(JsonHelper.Serialize(messages));
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
