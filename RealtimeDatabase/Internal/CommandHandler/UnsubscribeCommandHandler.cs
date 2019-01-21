using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeCommand>
    {
        public UnsubscribeCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, UnsubscribeCommand command)
        {
            await websocketConnection.Lock.WaitAsync();

            try
            {
                int index = websocketConnection.Subscriptions.FindIndex(s => s.ReferenceId == command.ReferenceId);

                if (index != -1)
                {
                    websocketConnection.Subscriptions.RemoveAt(index);
                }
            }
            finally
            {
                websocketConnection.Lock.Release();
            }
        }
    }
}
