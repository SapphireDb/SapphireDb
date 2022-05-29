using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        public SignalRConnection Connection { get; set; }
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

        public Task<ResponseBase> Handle(IConnectionInformation context, SubscribeCommand command,
            ExecutionContext executionContext)
        {
            if (databaseOptions.DisableIncludePrefilter && command.Prefilters.Any(p => p is IncludePrefilter))
            {
                throw new IncludeNotAllowedException(command.ContextName, command.CollectionName);
            }
            
            if (databaseOptions.DisableSelectPrefilter && command.Prefilters.Any(p => p is SelectPrefilter))
            {
                throw new SelectNotAllowedException(command.ContextName, command.CollectionName);
            }

            DbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = CollectionHelper.GetCollectionType(db, command);
            
            if (property.Key.GetModelAttributesInfo().DisableQueryAttribute != null ||
                databaseOptions.OnlyIncludedEntities && property.Key.GetModelAttributesInfo().IncludeEntityAttribute == null)
            {
                throw new OperationDisabledException("Query", command.ContextName, command.CollectionName);
            }
            
            command.Prefilters.ForEach(prefilter => prefilter.Initialize(property.Key));
            ResponseBase response = CollectionHelper.GetCollection(db, command, property, command.Prefilters, context, serviceProvider);

            subscriptionManager.AddSubscription(command.ContextName, command.CollectionName, command.Prefilters, Connection, command.ReferenceId);

            return Task.FromResult(response);
        }
    }
}
