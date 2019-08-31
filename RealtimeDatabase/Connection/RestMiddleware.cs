using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Connection
{
    class RestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly RealtimeDatabaseOptions options;
        private readonly RealtimeConnectionManager connectionManager;
        private readonly ILogger<RestMiddleware> logger;

        // ReSharper disable once UnusedParameter.Local
        public RestMiddleware(RequestDelegate next, RealtimeDatabaseOptions options, RealtimeConnectionManager connectionManager, ILogger<RestMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.connectionManager = connectionManager;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider, CommandExecutor commandExecutor)
        {
            string requestPath = context.Request.Path.Value.Substring(1).ToLowerInvariant();

            if (context.Request.Method != "POST" || string.IsNullOrEmpty(requestPath))
            {
                await next(context);
                return;
            }

            if (!string.IsNullOrEmpty(options.Secret))
            {
                if (context.Request.Headers["secret"] != options.Secret)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("The secret does not match");
                    return;
                }
            }

            if (!requestPath.EndsWith("command"))
            {
                requestPath += "command";
            }

            ConnectionBase connection = null;
            if (!string.IsNullOrEmpty(context.Request.Headers["connectionId"]))
            {
                Guid connectionId = Guid.Parse(context.Request.Headers["connectionId"]);
                connection = connectionManager.connections.FirstOrDefault(c => c.Id == connectionId);

                if (connection != null)
                {
                    ConnectionInfo connectionInfo = connection.HttpContext.Connection;

                    if (!connectionInfo.LocalIpAddress.Equals(context.Connection.LocalIpAddress) ||
                        !connectionInfo.LocalPort.Equals(context.Connection.LocalPort) ||
                        !connectionInfo.RemoteIpAddress.Equals(context.Connection.RemoteIpAddress))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("The connection does not match you origin");
                        return;
                    }
                }
            }

            StreamReader sr = new StreamReader(context.Request.Body);
            string requestBody = await sr.ReadToEndAsync();

            CommandBase command = JsonHelper.DeserializeCommand(requestBody);
            if (command != null)
            {
                if (command.GetType().Name.ToLowerInvariant() != requestPath)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("The specified path did not match the command type");
                    return;
                }

                ResponseBase response = await commandExecutor.ExecuteCommand(command,
                    serviceProvider.CreateScope().ServiceProvider, context, logger, connection);

                if (response?.Error != null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }

                await context.Response.WriteAsync(JsonHelper.Serialize(response));
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Parsing of the command was not successful");
            }
        }
    }
}
