using System;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.Subscribe
{
    class SubscribeCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private readonly IServiceProvider serviceProvider;
        private readonly SubscriptionManager subscriptionManager;
        private readonly SapphireDatabaseOptions databaseOptions;

        public SubscribeCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider,
            SubscriptionManager subscriptionManager, SapphireDatabaseOptions databaseOptions)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.subscriptionManager = subscriptionManager;
            this.databaseOptions = databaseOptions;
        }

        public Task<ResponseBase> Handle(HttpInformation context, SubscribeCommand command,
            ExecutionContext executionContext)
        {
            if (databaseOptions.DisableIncludePrefilter && command.Prefilters.Any(p => p is IncludePrefilter))
            {
                throw new IncludeNotAllowedException(command.CollectionName);
            }
            
            ResponseBase response = CollectionHelper.GetCollection(GetContext(command.ContextName), command, context, serviceProvider);

            subscriptionManager.AddSubscription(command.ContextName, command.CollectionName, command.Prefilters, Connection, command.ReferenceId);

            return Task.FromResult(response);
        }
    }
}
