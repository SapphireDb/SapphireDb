using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeRolesCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeRolesCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;

        public SubscribeRolesCommandHandler(AuthDbContextAccesor authDbContextAccessor, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
        }

        public async Task<ResponseBase> Handle(HttpContext context, SubscribeRolesCommand command)
        {
            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.RolesSubscription = command.ReferenceId;
            }
            finally
            {
                websocketConnection.Lock.Release();
            }

            return new SubscribeRolesResponse()
            {
                ReferenceId = command.ReferenceId,
                Roles = ModelHelper.GetRoles(GetContext()).ToList()
            };
        }

        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
