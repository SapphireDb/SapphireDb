using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeRolesCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeRolesCommand>
    {
        public UnsubscribeRolesCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public Task Handle(WebsocketConnection websocketConnection, UnsubscribeRolesCommand command)
        {
            lock (websocketConnection)
            {
                websocketConnection.RolesSubscription = null;
            }

            return Task.CompletedTask;
        }
    }
}
