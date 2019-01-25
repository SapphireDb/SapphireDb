using System;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public SubscribeCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
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
                await MessageHelper.SendCollection(GetContext(), command, websocketConnection, serviceProvider);
        }
    }
}
