using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeMessageCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;

        public UnsubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpContext context, UnsubscribeMessageCommand command)
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

            return null;
        }

        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
