using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.QueryQuery
{
    class QueryQueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryQueryCommand>, INeedsConnection
    {
        public SignalRConnection Connection { get; set; }
        private readonly IServiceProvider serviceProvider;
        private readonly SubscriptionManager subscriptionManager;
        private readonly SapphireDatabaseOptions _databaseOptions;

        public QueryQueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider,
            SubscriptionManager subscriptionManager, SapphireDatabaseOptions databaseOptions)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.subscriptionManager = subscriptionManager;
            _databaseOptions = databaseOptions;
        }

        public Task<ResponseBase> Handle(IConnectionInformation context, QueryQueryCommand queryCommand,
            ExecutionContext executionContext)
        {
            DbContext db = GetContext(queryCommand.ContextName);
            KeyValuePair<Type, string> property = CollectionHelper.GetCollectionType(db, queryCommand);

            if (_databaseOptions.OnlyIncludedEntities && property.Key.GetModelAttributesInfo().IncludeEntityAttribute == null)
            {
                throw new OperationDisabledException("QueryQuery", queryCommand.ContextName, queryCommand.CollectionName);
            }
            
            List<IPrefilterBase> prefilters =
                CollectionHelper.GetQueryPrefilters(property, queryCommand, Connection, serviceProvider);
            
            ResponseBase response = CollectionHelper.GetCollection(db, queryCommand, property, prefilters, context, serviceProvider);
            return Task.FromResult(response);
        }
    }
}