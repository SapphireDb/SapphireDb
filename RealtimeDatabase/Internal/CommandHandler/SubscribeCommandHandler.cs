using System;
using System.Collections.Generic;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeCommand>, INeedsWebsocket
    {
        private WebsocketConnection websocketConnection;
        private readonly IServiceProvider serviceProvider;

        public SubscribeCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<ResponseBase> Handle(HttpContext context, SubscribeCommand command)
        {
            CollectionSubscription collectionSubscription = new CollectionSubscription()
            {
                CollectionName = command.CollectionName.ToLowerInvariant(),
                ContextName = command.ContextName.ToLowerInvariant(),
                ReferenceId = command.ReferenceId,
                Prefilters = command.Prefilters
            };

            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.Subscriptions.Add(collectionSubscription);
            }
            finally
            {
                websocketConnection.Lock.Release();
            }

            ResponseBase response = MessageHelper.GetCollection(GetContext(command.ContextName), command, context, serviceProvider, out List<object[]> transmittedData);

            await collectionSubscription.Lock.WaitAsync();
            collectionSubscription.TransmittedData = transmittedData;
            collectionSubscription.Lock.Release();

            return response;
        }


        public void InsertWebsocket(WebsocketConnection currentWebsocketConnection)
        {
            websocketConnection = currentWebsocketConnection;
        }
    }
}
