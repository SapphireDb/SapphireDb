using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.QueryConnections
{
    class QueryConnectionsCommandHandler : CommandHandlerBase, ICommandHandler<QueryConnectionsCommand>
    {
        private readonly ConnectionManager connectionManager;

        public QueryConnectionsCommandHandler(DbContextAccesor dbContextAccessor, ConnectionManager connectionManager)
            : base(dbContextAccessor)
        {
            this.connectionManager = connectionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, QueryConnectionsCommand command)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                string userId = context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    QueryConnectionsResponse response = new QueryConnectionsResponse()
                    {
                        Connections = connectionManager.connections.Where(c => c.Information.UserId == userId).ToList(),
                        ReferenceId = command.ReferenceId
                    };

                    return Task.FromResult<ResponseBase>(response);
                }
            }

            return Task.FromResult(command.CreateExceptionResponse<QueryConnectionsResponse>("Not authenticated"));
        }
    }
}
