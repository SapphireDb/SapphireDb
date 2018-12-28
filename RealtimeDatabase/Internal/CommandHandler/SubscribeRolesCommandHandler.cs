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
    class SubscribeRolesCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeRolesCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public SubscribeRolesCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, SubscribeRolesCommand command)
        {
            lock (websocketConnection)
            {
                websocketConnection.RolesSubscription = command.ReferenceId;
            }

            await SendMessage(websocketConnection, new SubscribeRolesResponse()
            {
                ReferenceId = command.ReferenceId,
                Roles = ModelHelper.GetRoles(GetContext()).ToList()
            });
        }
    }
}
