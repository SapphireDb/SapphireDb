using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class DeleteRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<DeleteRoleCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly WebsocketConnectionManager connectionManager;

        public DeleteRoleCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, WebsocketConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
            this.connectionManager = connectionManager;
        }

        public async Task Handle(WebsocketConnection websocketConnection, DeleteRoleCommand command)
        {
            IdentityRole role = await roleManager.FindByIdAsync(command.Id);

            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    await SendDataToClients(role, websocketConnection, command);
                }
                else
                {
                    await websocketConnection.Send(new DeleteRoleResponse
                    {
                        ReferenceId = command.ReferenceId,
                        IdentityErrors = result.Errors
                    });
                }
            }
            else
            {
                await websocketConnection.SendException<DeleteRoleResponse>(command, "Role not found");
            }            
        }

        private async Task SendDataToClients(IdentityRole role, WebsocketConnection websocketConnection, DeleteRoleCommand command)
        {
            IRealtimeAuthContext db = GetContext();
            db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.RoleId == role.Id));
            db.SaveChanges();

            await websocketConnection.Send(new DeleteRoleResponse()
            {
                ReferenceId = command.ReferenceId
            });

            await MessageHelper.SendRolesUpdate(db, connectionManager);

            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);
            await MessageHelper.SendUsersUpdate(db, contextTypeContainer, usermanager, connectionManager);
        }
    }
}
