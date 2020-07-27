using System;
using System.Threading.Tasks;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.SubscribeQuery
{
    class SubscribeQueryCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeQueryCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private readonly IServiceProvider serviceProvider;
        private readonly SubscriptionManager subscriptionManager;
        
        public SubscribeQueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider,
            SubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, SubscribeQueryCommand command,
            ExecutionContext executionContext)
        {
            // ResponseBase response = CollectionHelper.GetCollection(GetContext(command.ContextName), command, context, serviceProvider);

            // if (response.Error == null)
            // {
                // subscriptionManager.AddSubscription(command.ContextName, command.CollectionName, command.Prefilters,
                    // Connection, command.ReferenceId);
            // }

            // return Task.FromResult(response);

            return null;
        }
    }
}