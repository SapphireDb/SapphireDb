using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeUsersCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeUsersCommand>
    {
        public UnsubscribeUsersCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, UnsubscribeUsersCommand command)
        {
            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.UsersSubscription = null;
            }
            finally
            {
                websocketConnection.Lock.Release();
            }
        }
    }
}
