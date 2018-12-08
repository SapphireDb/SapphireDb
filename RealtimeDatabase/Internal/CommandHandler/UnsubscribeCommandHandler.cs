using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeCommand>
    {
        public UnsubscribeCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public Task Handle(WebsocketConnection websocketConnection, UnsubscribeCommand command)
        {
            lock (websocketConnection)
            {
                websocketConnection.Subscriptions.RemoveAt(
                    websocketConnection.Subscriptions.FindIndex(s => s.ReferenceId == command.ReferenceId));
            }

            return Task.CompletedTask;
        }
    }
}
