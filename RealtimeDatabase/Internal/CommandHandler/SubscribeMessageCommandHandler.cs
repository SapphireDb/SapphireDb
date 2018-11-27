using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeMessageCommand>
    {
        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection)
            : base(dbContextAccesor, websocketConnection)
        {

        }

        public Task Handle(SubscribeMessageCommand command)
        {
            lock (websocketConnection)
            {
                websocketConnection.MessageSubscriptions.Add(command.ReferenceId, command.Topic);
            }

            return Task.CompletedTask;
        }
    }
}
