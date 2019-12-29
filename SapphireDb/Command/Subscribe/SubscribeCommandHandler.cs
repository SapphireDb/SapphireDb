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

        public SubscribeCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, SubscribeCommand command)
        {
            ResponseBase response = CollectionHelper.GetCollection(GetContext(command.ContextName), command, context, serviceProvider);

            if (response.Error == null)
            {
                CollectionSubscription collectionSubscription = new CollectionSubscription()
                {
                    CollectionName = command.CollectionName.ToLowerInvariant(),
                    ContextName = command.ContextName.ToLowerInvariant(),
                    ReferenceId = command.ReferenceId,
                    Prefilters = command.Prefilters
                };

                await Connection.AddSubscription(collectionSubscription);   
            }

            return response;
        }
    }
}
