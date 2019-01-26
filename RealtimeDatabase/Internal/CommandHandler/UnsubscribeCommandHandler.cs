using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;

        public UnsubscribeCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpContext context, UnsubscribeCommand command)
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

            return null;
        }

        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
