using System;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.CloseConnection
{
    class CloseConnectionCommandHandler : AuthCommandHandlerBase, ICommandHandler<CloseConnectionCommand>
    {
        private readonly ConnectionManager connectionManager;

        public CloseConnectionCommandHandler(AuthDbContextAccesor authDbContextAccessor, IServiceProvider serviceProvider, ConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.connectionManager = connectionManager;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, CloseConnectionCommand command)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                string userId = context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    ConnectionBase connectionToClose = connectionManager.connections.FirstOrDefault(c => c.Information.UserId == userId && c.Id == command.ConnectionId);

                    if (connectionToClose != null)
                    {
                        if (command.DeleteRenewToken)
                        {
                            ISapphireAuthContext db = GetContext();
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
