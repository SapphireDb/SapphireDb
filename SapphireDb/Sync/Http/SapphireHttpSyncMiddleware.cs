using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SapphireDb.Helper;
using SapphireDb.Models;
using SapphireDb.Sync.Models;

namespace SapphireDb.Sync.Http
{
    class SapphireHttpSyncMiddleware
    {
        private readonly RequestDelegate next;
        private readonly SapphireDatabaseOptions options;
        private readonly ILogger<SapphireHttpSyncMiddleware> logger;
        private readonly SapphireHttpSyncModule sapphireHttpSyncModule;

        public SapphireHttpSyncMiddleware(
            RequestDelegate next,
            SapphireDatabaseOptions options,
            ILogger<SapphireHttpSyncMiddleware> logger,
            ISapphireSyncModule sapphireHttpSyncModule)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
            this.sapphireHttpSyncModule = (SapphireHttpSyncModule)sapphireHttpSyncModule;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method != "POST")
            {
                await next(context);
                return;
            }

            logger.LogInformation("Started handling sync request");

            string originId = context.Request.Headers["OriginId"].ToString();
            if (context.Request.Headers["Secret"].ToString().ComputeHash() != options.Sync.Secret ||
                options.Sync.Entries.All(e => e.Id != originId))
            {
                logger.LogError("Prevented unauthorized access to nlb sync methods");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("You are not allowed to access this endpoint");
                return;
            }

            StreamReader sr = new StreamReader(context.Request.Body);
            string requestBody = await sr.ReadToEndAsync();

            string requestPath = context.Request.Path.Value.Split('/').LastOrDefault();

            switch (requestPath)
            {
                case "changes":
                    SendChangesRequest changesRequest = JsonConvert.DeserializeObject<SendChangesRequest>(requestBody);
                    sapphireHttpSyncModule.Received(changesRequest);
                    break;
                case "publish":
                    SendPublishRequest publishRequest = JsonConvert.DeserializeObject<SendPublishRequest>(requestBody);
                    sapphireHttpSyncModule.Received(publishRequest);
                    break;
                case "message":
                    SendMessageRequest messageRequest = JsonConvert.DeserializeObject<SendMessageRequest>(requestBody);
                    sapphireHttpSyncModule.Received(messageRequest);
                    break;
            }
        }
    }
}