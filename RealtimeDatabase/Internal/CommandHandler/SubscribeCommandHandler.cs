using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeCommand>
    {
        public SubscribeCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {
        }

        public async Task Handle(WebsocketConnection websocketConnection, SubscribeCommand command)
        {
            CollectionSubscription collectionSubscription = new CollectionSubscription()
            {
                CollectionName = command.CollectionName.ToLowerInvariant(),
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

            collectionSubscription.TransmittedData =
                await MessageHelper.SendCollection(GetContext(), command, websocketConnection);
        }
    }
}
