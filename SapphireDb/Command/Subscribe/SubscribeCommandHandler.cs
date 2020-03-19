using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Subscribe
{
    class SubscribeCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private readonly IServiceProvider serviceProvider;
        private readonly SubscriptionManager subscriptionManager;

        public SubscribeCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider,
            SubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, SubscribeCommand command)
        {
            ResponseBase response = CollectionHelper.GetCollection(GetContext(command.ContextName), command, context, serviceProvider);

            if (response.Error == null)
            {
                CollectionSubscription collectionSubscription = new CollectionSubscription()
                {
                    CollectionName = command.CollectionName.ToLowerInvariant(),
                    ContextName = command.ContextName.ToLowerInvariant(),
                    ReferenceId = command.ReferenceId,
                    Prefilters = command.Prefilters,
                    Connection = Connection
                };

                subscriptionManager.AddSubscription(collectionSubscription);
            }

            return Task.FromResult(response);
        }
    }
}
