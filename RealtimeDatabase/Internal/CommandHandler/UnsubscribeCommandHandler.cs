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
        public UnsubscribeCommandHandler(DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection)
            : base(dbContextAccesor, websocketConnection)
        {

        }

        public Task Handle(UnsubscribeCommand command)
        {
            websocketConnection.Subscriptions.RemoveAt(
                websocketConnection.Subscriptions.FindIndex(s => s.ReferenceId == command.ReferenceId));

            return Task.CompletedTask;
        }
    }
}
