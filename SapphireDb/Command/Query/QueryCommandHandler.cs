using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.Query
{
    class QueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly SapphireDatabaseOptions databaseOptions;

        public QueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider, SapphireDatabaseOptions databaseOptions)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.databaseOptions = databaseOptions;
        }

        public Task<ResponseBase> Handle(HttpInformation context, QueryCommand command,
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
            
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = CollectionHelper.GetCollectionType(db, command);
            
            if (property.Key.GetModelAttributesInfo().DisableQueryAttribute != null)
            {
                throw new OperationDisabledException("Query", command.ContextName, command.CollectionName);
            }
            
            command.Prefilters.ForEach(prefilter => prefilter.Initialize(property.Key));
            return Task.FromResult(CollectionHelper.GetCollection(db, command, property, command.Prefilters, context, serviceProvider));
        }
    }
}
