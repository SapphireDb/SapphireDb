using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SapphireDb.Command;
using SapphireDb.Command.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    class RestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly SapphireDatabaseOptions options;
        private readonly ConnectionManager connectionManager;
        private readonly ILogger<RestMiddleware> logger;

        // ReSharper disable once UnusedParameter.Local
        public RestMiddleware(RequestDelegate next, SapphireDatabaseOptions options, ConnectionManager connectionManager, ILogger<RestMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.connectionManager = connectionManager;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider, CommandExecutor commandExecutor)
        {
            connectionManager.CheckExistingConnections();

            string requestPath = context.Request.Path.Value.Substring(1).ToLowerInvariant();

            if (context.Request.Method != "POST" || string.IsNullOrEmpty(requestPath))
            {
                await next(context);
                return;
            }

            if (!AuthHelper.CheckApiAuth(context.Request.Headers["key"], context.Request.Headers["secret"], options))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(JsonHelper.Serialize(new WrongApiResponse()));
                return;
            }

            if (!requestPath.EndsWith("command"))
            {
                requestPath += "command";
            }

            ConnectionBase connection = connectionManager.GetConnection(context);

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
                    serviceProvider.CreateScope().ServiceProvider, connection != null ? connection.Information : new HttpInformation(context), logger, connection);

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
