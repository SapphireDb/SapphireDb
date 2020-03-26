using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Command.Connection;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Connection.Poll
{
    class PollMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ConnectionManager connectionManager;
        private readonly SapphireDatabaseOptions options;
        
        public PollMiddleware(RequestDelegate next, ConnectionManager connectionManager, SapphireDatabaseOptions options)
        {
            this.next = next;
            this.connectionManager = connectionManager;
            this.options = options;
        }

        public async Task Invoke(HttpContext context)
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
