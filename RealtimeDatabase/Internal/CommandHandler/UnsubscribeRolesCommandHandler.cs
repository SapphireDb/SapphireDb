using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeRolesCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeRolesCommand>
    {
        public UnsubscribeRolesCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, UnsubscribeRolesCommand command)
        {
            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.RolesSubscription = null;
            }
            finally
            {
                websocketConnection.Lock.Release();
            }
        }
    }
}
