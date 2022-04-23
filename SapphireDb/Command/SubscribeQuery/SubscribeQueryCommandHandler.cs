using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Command.SubscribeQuery
{
    class SubscribeQueryCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeQueryCommand>, INeedsConnection
    {
        public SignalRConnection Connection { get; set; }
        private readonly IServiceProvider serviceProvider;
        private readonly SubscriptionManager subscriptionManager;

        public SubscribeQueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider,
            SubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(IConnectionInformation context, SubscribeQueryCommand queryCommand,
            ExecutionContext executionContext)
        {
            DbContext db = GetContext(queryCommand.ContextName);
            KeyValuePair<Type, string> property = CollectionHelper.GetCollectionType(db, queryCommand);

            List<IPrefilterBase> prefilters =
                CollectionHelper.GetQueryPrefilters(property, queryCommand, Connection, serviceProvider);
            
            ResponseBase response = CollectionHelper.GetCollection(db, queryCommand, property, prefilters, context, serviceProvider);
            
            subscriptionManager.AddSubscription(queryCommand.ContextName, queryCommand.CollectionName, prefilters, Connection, queryCommand.ReferenceId);

            return Task.FromResult(response);
        }
    }
}