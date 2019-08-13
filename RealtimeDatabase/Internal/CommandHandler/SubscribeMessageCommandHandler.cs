using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeMessageCommand>, INeedsWebsocket
    {
        private readonly RealtimeDatabaseOptions options;
        private WebsocketConnection websocketConnection;

        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor, RealtimeDatabaseOptions options)
            : base(dbContextAccessor)
        {
            this.options = options;
        }

        public async Task<ResponseBase> Handle(HttpContext context, SubscribeMessageCommand command)
        {
            if (!options.IsAllowedForTopicSubscribe(context, command.Topic))
            {
                return command.CreateExceptionResponse<ResponseBase>("Not allowed to subscribe this topic");
            }

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
