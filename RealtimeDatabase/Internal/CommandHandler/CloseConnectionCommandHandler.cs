using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CloseConnectionCommandHandler : AuthCommandHandlerBase, ICommandHandler<CloseConnectionCommand>, IRestFallback
    {
        private readonly RealtimeConnectionManager connectionManager;

        public CloseConnectionCommandHandler(AuthDbContextAccesor authDbContextAccessor, IServiceProvider serviceProvider, RealtimeConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.connectionManager = connectionManager;
        }

        public async Task<ResponseBase> Handle(HttpContext context, CloseConnectionCommand command)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                string userId = context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    ConnectionBase connectionToClose = connectionManager.connections.FirstOrDefault(c => c.UserId == userId && c.Id == command.ConnectionId);

                    if (connectionToClose != null)
                    {
                        if (command.DeleteRenewToken)
                        {
                            string sessionId = connectionToClose.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "SessionId")?.Value;
                            IRealtimeAuthContext db = GetContext();
                            db.RefreshTokens.RemoveRange(db.RefreshTokens.Where(rt => rt.UserId == userId));
                        }

                        await connectionToClose.Close();

                        CloseConnectionResponse response = new CloseConnectionResponse()
                        {
                            ReferenceId = command.ReferenceId
                        };

                        return response;
                    }

                    return command.CreateExceptionResponse<CloseConnectionResponse>("No connection to close was found");
                }
            }

            return command.CreateExceptionResponse<CloseConnectionResponse>("Not authenticated");
        }
    }
}
