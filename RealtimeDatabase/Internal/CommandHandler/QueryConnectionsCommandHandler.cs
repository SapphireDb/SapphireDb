using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Websocket;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class QueryConnectionsCommandHandler : CommandHandlerBase, ICommandHandler<QueryConnectionsCommand>, IRestFallback
    {
        private readonly WebsocketConnectionManager connectionManager;

        public QueryConnectionsCommandHandler(DbContextAccesor dbContextAccessor, WebsocketConnectionManager connectionManager)
            : base(dbContextAccessor)
        {
            this.connectionManager = connectionManager;
        }

        public Task<ResponseBase> Handle(HttpContext context, QueryConnectionsCommand command)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                string userId = context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    QueryConnectionsResponse response = new QueryConnectionsResponse()
                    {
                        Connections = connectionManager.connections.Where(c => c.UserId == userId).ToList(),
                        ReferenceId = command.ReferenceId
                    };

                    return Task.FromResult<ResponseBase>(response);
                }
            }

            return Task.FromResult(command.CreateExceptionResponse<QueryConnectionsResponse>("Not authenticated"));
        }
    }
}
