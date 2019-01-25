using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeUsersCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeUsersCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public SubscribeUsersCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, SubscribeUsersCommand command)
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

            await websocketConnection.Send(new SubscribeUsersResponse()
            {
                ReferenceId = command.ReferenceId,
                Users = ModelHelper.GetUsers(GetContext(), contextTypeContainer, usermanager).ToList()
            });
        }
    }
}
