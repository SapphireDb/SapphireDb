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
        public UnsubscribeMessageCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public Task Handle(WebsocketConnection websocketConnection, UnsubscribeMessageCommand command)
        {
            lock (websocketConnection)
            {
                websocketConnection.MessageSubscriptions.Remove(command.ReferenceId);
            }

            return Task.CompletedTask;
        }
    }
}
