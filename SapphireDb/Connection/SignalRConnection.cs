using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using SapphireDb.Command;
using SapphireDb.Helper;

namespace SapphireDb.Connection
{
    public class SignalRConnection : IConnectionInformation
    {
        private readonly HubCallerContext _hubCallerContext;

        public SignalRConnection(HubCallerContext hubCallerContext)
        {
            _hubCallerContext = hubCallerContext;

            ClientVersion = HttpContext.Request.Query["ClientVersion"];
            ApiKey = HttpContext.Request.Query["ApiKey"];
        }

        public string Id => _hubCallerContext.ConnectionId;

        public string ClientVersion { get; }

        public string ApiKey { get; }

        public ClaimsPrincipal User => _hubCallerContext.User;
        public HttpContext HttpContext => _hubCallerContext.GetHttpContext();

        public async Task Send(ResponseBase response, IServiceProvider serviceProvider)
        {
            IHubContext<SapphireDbHub> hub = serviceProvider.GetRequiredService<IHubContext<SapphireDbHub>>();
            
            JToken responseJson = JsonHelper.Serialize(response);
            await hub.Clients.Client(Id).SendAsync("ServerMessage", responseJson);
        }
    }
}