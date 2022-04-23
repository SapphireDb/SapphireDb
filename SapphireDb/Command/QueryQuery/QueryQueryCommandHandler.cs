using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Command.QueryQuery
{
    class QueryQueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryQueryCommand>, INeedsConnection
    {
        public SignalRConnection Connection { get; set; }
        private readonly IServiceProvider serviceProvider;
        private readonly SubscriptionManager subscriptionManager;

        public QueryQueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider,
            SubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(IConnectionInformation context, QueryQueryCommand queryCommand,
            ExecutionContext executionContext)
        {
            DbContext db = GetContext(queryCommand.ContextName);
            KeyValuePair<Type, string> property = CollectionHelper.GetCollectionType(db, queryCommand);

            List<IPrefilterBase> prefilters =
                CollectionHelper.GetQueryPrefilters(property, queryCommand, Connection, serviceProvider);
            
            ResponseBase response = CollectionHelper.GetCollection(db, queryCommand, property, prefilters, context, serviceProvider);
            return Task.FromResult(response);
        }
    }
}