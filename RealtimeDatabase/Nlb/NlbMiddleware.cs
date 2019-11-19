using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Nlb.Models;

namespace RealtimeDatabase.Nlb
{
    class NlbMiddleware
    {
        private readonly RequestDelegate next;
        private readonly RealtimeDatabaseOptions options;
        private readonly ILogger<NlbMiddleware> logger;

        // ReSharper disable once UnusedParameter.Local
        public NlbMiddleware(RequestDelegate next, RealtimeDatabaseOptions options, ILogger<NlbMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context, RealtimeChangeNotifier changeNotifier)
        {
            if (context.Request.Method != "POST")
            {
                await next(context);
                return;
            }

            logger.LogInformation("Started handling nlb message");

            if (context.Request.Headers["Secret"].ToString().ComputeHash() != options.Nlb.Secret ||
                /*context.Connection.RemoteIpAddress*/ false)
            {
                logger.LogError("Prevented unauthorized access to nlb sync methods");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("You are not allowed to access this endpoint");
                return;
            }

            StreamReader sr = new StreamReader(context.Request.Body);
            string requestBody = await sr.ReadToEndAsync();
            requestBody = requestBody.Decrypt(options.Nlb.EncryptionKey);

            SendChangesRequest request = JsonConvert.DeserializeObject<SendChangesRequest>(requestBody);

            Type dbType = Assembly.GetEntryAssembly()?.DefinedTypes.FirstOrDefault(t => t.FullName == request.DbType);

            if (dbType != null)
            {
                changeNotifier.HandleChanges(request.Changes, dbType);
            }

            //CommandBase command = JsonHelper.DeserializeCommand(requestBody);
            //if (command != null)
            //{
            //    if (command.GetType().Name.ToLowerInvariant() != requestPath)
            //    {
            //        context.Response.StatusCode = StatusCodes.Status400BadRequest;
            //        await context.Response.WriteAsync("The specified path did not match the command type");
            //        return;
            //    }

            //    ResponseBase response = await commandExecutor.ExecuteCommand(command,
            //        serviceProvider.CreateScope().ServiceProvider, context, logger, connection);

            //    if (response?.Error != null)
            //    {
            //        context.Response.StatusCode = StatusCodes.Status400BadRequest;
            //    }

            //    await context.Response.WriteAsync(JsonHelper.Serialize(response));
            //}
            //else
            //{
            //    context.Response.StatusCode = StatusCodes.Status400BadRequest;
            //    await context.Response.WriteAsync("Parsing of the command was not successful");
            //}
        }
    }
}
