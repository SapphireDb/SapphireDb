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
        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, SubscribeMessageCommand command)
        {
            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.MessageSubscriptions.Add(command.ReferenceId, command.Topic);
            }
            finally
            {
                websocketConnection.Lock.Release();
            }
        }
    }
}
