using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Models;
using SapphireDb.Nlb.Models;

namespace SapphireDb.Nlb
{
    class NlbMiddleware
    {
        private readonly RequestDelegate next;
        private readonly SapphireDatabaseOptions options;
        private readonly ILogger<NlbMiddleware> logger;

        // ReSharper disable once UnusedParameter.Local
        public NlbMiddleware(RequestDelegate next, SapphireDatabaseOptions options, ILogger<NlbMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context, SapphireChangeNotifier changeNotifier, SapphireMessageSender sender)
        {
            if (context.Request.Method != "POST")
            {
                await next(context);
                return;
            }

            logger.LogInformation("Started handling nlb message");

            string originId = context.Request.Headers["OriginId"].ToString();
            if (context.Request.Headers["Secret"].ToString().ComputeHash() != options.Nlb.Secret ||
                options.Nlb.Entries.All(e => e.Id != originId))
            {
                logger.LogError("Prevented unauthorized access to nlb sync methods");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("You are not allowed to access this endpoint");
                return;
            }

            StreamReader sr = new StreamReader(context.Request.Body);
            string requestBody = await sr.ReadToEndAsync();
            requestBody = requestBody.Decrypt(options.Nlb.EncryptionKey);

            string requestPath = context.Request.Path.Value.Split('/').LastOrDefault();

            switch (requestPath)
            {
                case "changes":
                    SendChangesRequest changesRequest = JsonConvert.DeserializeObject<SendChangesRequest>(requestBody);

                    Type dbType = Assembly.GetEntryAssembly()?.DefinedTypes.FirstOrDefault(t => t.FullName == changesRequest.DbType);

                    if (dbType != null)
                    {
                        logger.LogInformation("Handling changes from other server");
                        changeNotifier.HandleChanges(changesRequest.Changes, dbType);
                    }
                    break;
                case "publish":
                    SendPublishRequest publishRequest = JsonConvert.DeserializeObject<SendPublishRequest>(requestBody);
                    logger.LogInformation("Handling publish from other server");
                    sender.Publish(publishRequest.Topic, publishRequest.Message, true);
                    break;
                case "message":
                    SendMessageRequest messageRequest = JsonConvert.DeserializeObject<SendMessageRequest>(requestBody);
                    logger.LogInformation("Handling message from other server");
                    sender.Send(messageRequest.Message, true);
                    break;
            }
        }
    }
}
