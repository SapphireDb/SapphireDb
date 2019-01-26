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
    class SubscribeUsersCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeUsersCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public SubscribeUsersCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task<ResponseBase> Handle(HttpContext context, SubscribeUsersCommand command)
        {
            object usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.UsersSubscription = command.ReferenceId;
            }
            finally
            {
                websocketConnection.Lock.Release();
            }

            return new SubscribeUsersResponse()
            {
                ReferenceId = command.ReferenceId,
                Users = ModelHelper.GetUsers(GetContext(), contextTypeContainer, usermanager).ToList()
            };
        }

        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
