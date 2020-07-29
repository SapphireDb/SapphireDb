using System;
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
                throw new IncludeNotAllowedException(command.CollectionName);
            }
            
            return Task.FromResult(CollectionHelper.GetCollection(GetContext(command.ContextName), command, command.Prefilters, context, serviceProvider));
        }
    }
}
