using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeUsersCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeUsersCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;

        public UnsubscribeUsersCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpContext context, UnsubscribeUsersCommand command)
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

            return null;
        }

        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
