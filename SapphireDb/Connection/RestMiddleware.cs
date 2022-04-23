using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SapphireDb.Command;
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
        
        public RestMiddleware(RequestDelegate next, SapphireDatabaseOptions options, ConnectionManager connectionManager, ILogger<RestMiddleware> logger)
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

            if (!requestPath.EndsWith("command"))
            {
                requestPath += "command";
            }

            StreamReader sr = new StreamReader(context.Request.Body);
            string requestBody = await sr.ReadToEndAsync();
            
            CommandBase command = JsonHelper.DeserializeCommand(new JObject(requestBody));
            if (command != null)
            {
                if (command.GetType().Name.ToLowerInvariant() != requestPath)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("The specified path did not match the command type");
                    return;
                }

                ResponseBase response = await commandExecutor.ExecuteCommand(command,
                    serviceProvider.CreateScope().ServiceProvider, new HttpConnectionInformation(context), logger);

                if (response?.Error != null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }

                await context.Response.WriteAsync(JsonHelper.Serialize(response).ToString());
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Parsing of command was not successful");
            }
        }
    }
}
