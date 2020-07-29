using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Attributes;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;
using SapphireDb.Models.SapphireApiBuilder;

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
            SapphireDbContext db = GetContext(command.ContextName);
            Type dbContextType = db.GetType();
            KeyValuePair<Type, string> property = dbContextType.GetDbSetType(command.CollectionName);

            if (property.Key == null)
            {
                throw new CollectionNotFoundException(command.ContextName, command.CollectionName);
            }

            QueryAttribute query = property.Key.GetModelAttributesInfo()
                .QueryAttributes
                .FirstOrDefault(q =>
                    q.QueryName.Equals(command.QueryName, StringComparison.InvariantCultureIgnoreCase));

            if (query == null)
            {
                throw new QueryNotFoundException(command.ContextName, command.CollectionName, command.QueryName);
            }

            dynamic queryBuilder =
                Activator.CreateInstance(typeof(SapphireQueryBuilder<>).MakeGenericType(property.Key));

            if (query.FunctionLambda != null)
            {
                queryBuilder = query.FunctionLambda(queryBuilder, Connection.Information, command.Parameters);
            }

            if (query.FunctionInfo != null)
            {
                queryBuilder = query.FunctionInfo.Invoke(null,
                    new object[] {queryBuilder, Connection.Information, command.Parameters});
            }

            List<IPrefilterBase> prefilters = typeof(SapphireQueryBuilderBase<>)
                .MakeGenericType(property.Key)
                .GetField("prefilters")
                .GetValue(queryBuilder);
            
            
            ResponseBase response = CollectionHelper.GetCollection(db, command, prefilters, context, serviceProvider);

            subscriptionManager.AddSubscription(command.ContextName, command.CollectionName, prefilters, Connection, command.ReferenceId);

            return Task.FromResult(response);
        }
    }
}