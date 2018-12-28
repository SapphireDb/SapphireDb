using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeUsersCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeUsersCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public SubscribeUsersCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, SubscribeUsersCommand command)
        {
            object usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            lock (websocketConnection)
            {
                websocketConnection.UsersSubscription = command.ReferenceId;
            }

            await SendMessage(websocketConnection, new SubscribeUsersResponse()
            {
                ReferenceId = command.ReferenceId,
                Users = ModelHelper.GetUsers(GetContext(), contextTypeContainer, usermanager).ToList()
            });
        }
    }
}
