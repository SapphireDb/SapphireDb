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

        public async Task Handle(WebsocketConnection websocketConnection, UnsubscribeMessageCommand command)
        {
            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.MessageSubscriptions.Remove(command.ReferenceId);
            }
            finally
            {
                websocketConnection.Lock.Release();
            }
        }
    }
}
