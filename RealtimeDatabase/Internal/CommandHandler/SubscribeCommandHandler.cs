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
        public SubscribeCommandHandler(DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection)
            : base(dbContextAccesor, websocketConnection)
        {

        }

        public async Task Handle(SubscribeCommand command)
        {
            CollectionSubscription collectionSubscription = new CollectionSubscription()
            {
                CollectionName = command.CollectionName.ToLowerInvariant(),
                ReferenceId = command.ReferenceId,
                Prefilters = command.Prefilters
            };

            websocketConnection.Subscriptions.Add(collectionSubscription);

            collectionSubscription.TransmittedData =
                await new QueryCommandHandler(contextAccesor, websocketConnection).HandleInner(command);
        }
    }
}
