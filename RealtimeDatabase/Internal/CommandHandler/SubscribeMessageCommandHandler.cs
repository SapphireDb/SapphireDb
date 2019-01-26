using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeMessageCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;

        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpContext context, SubscribeMessageCommand command)
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

            return null;
        }

        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
