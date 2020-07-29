using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Attributes;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

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
                .FirstOrDefault(q => q.QueryName.Equals(command.QueryName, StringComparison.InvariantCultureIgnoreCase));

            if (query == null)
            {
                throw new QueryNotFoundException(command.ContextName, command.CollectionName, command.QueryName);
            }
            
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