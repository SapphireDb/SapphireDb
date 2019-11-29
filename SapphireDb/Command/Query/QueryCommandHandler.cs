using System;
using System.Threading.Tasks;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Query
{
    class QueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public QueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpInformation context, QueryCommand command)
        {
            return Task.FromResult(MessageHelper.GetCollection(GetContext(command.ContextName), command, context, serviceProvider, out _));
        }
    }
}
