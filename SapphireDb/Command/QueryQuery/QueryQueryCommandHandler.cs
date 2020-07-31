using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Command.QueryQuery
{
    class QueryQueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryQueryCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private readonly IServiceProvider serviceProvider;
        private readonly SubscriptionManager subscriptionManager;

        public QueryQueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider,
            SubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, QueryQueryCommand queryCommand,
            ExecutionContext executionContext)
        {
            SapphireDbContext db = GetContext(queryCommand.ContextName);
            KeyValuePair<Type, string> property = CollectionHelper.GetCollectionType(db, queryCommand);

            List<IPrefilterBase> prefilters =
                CollectionHelper.GetQueryPrefilters(property, queryCommand, Connection.Information, serviceProvider);
            
            ResponseBase response = CollectionHelper.GetCollection(db, queryCommand, property, prefilters, context, serviceProvider);
            
            subscriptionManager.AddSubscription(queryCommand.ContextName, queryCommand.CollectionName, prefilters, Connection, queryCommand.ReferenceId);

            return Task.FromResult(response);
        }
    }
}