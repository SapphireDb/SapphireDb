using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeMessageCommand>
    {
        public UnsubscribeMessageCommandHandler(DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection)
            : base(dbContextAccesor, websocketConnection)
        {

        }

        public Task Handle(UnsubscribeMessageCommand command)
        {
            lock (websocketConnection)
            {
                websocketConnection.MessageSubscriptions.Remove(command.ReferenceId);
            }

            return Task.CompletedTask;
        }
    }
}
