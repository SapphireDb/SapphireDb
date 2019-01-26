using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeRolesCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeRolesCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;

        public UnsubscribeRolesCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpContext context, UnsubscribeRolesCommand command)
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

            return null;
        }

        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
