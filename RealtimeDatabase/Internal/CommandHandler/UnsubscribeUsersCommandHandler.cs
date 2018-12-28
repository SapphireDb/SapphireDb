using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeUsersCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeUsersCommand>
    {
        public UnsubscribeUsersCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public Task Handle(WebsocketConnection websocketConnection, UnsubscribeUsersCommand command)
        {
            lock (websocketConnection)
            {
                websocketConnection.UsersSubscription = null;
            }

            return Task.CompletedTask;
        }
    }
}
