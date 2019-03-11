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
    class CloseConnectionCommandHandler : AuthCommandHandlerBase, ICommandHandler<CloseConnectionCommand>, IRestFallback
    {
        private readonly WebsocketConnectionManager connectionManager;

        public CloseConnectionCommandHandler(AuthDbContextAccesor authDbContextAccessor, IServiceProvider serviceProvider, WebsocketConnectionManager connectionManager)
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
                    WebsocketConnection connectionToClose = connectionManager.connections.FirstOrDefault(c => c.UserId == userId && c.Id == command.ConnectionId);

                    if (connectionToClose != null)
                    {
                        if (command.DeleteRenewToken)
                        {
                            string sessionId = connectionToClose.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "SessionId")?.Value;
                            IRealtimeAuthContext db = GetContext();
                            db.RefreshTokens.RemoveRange(db.RefreshTokens.Where(rt => rt.UserId == userId));
                        }

                        await connectionToClose.Websocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "The connection was closed by another client", WebsocketHelper.token);

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
