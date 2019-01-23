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
    class UpdateRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<UpdateRoleCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly WebsocketConnectionManager connectionManager;

        public UpdateRoleCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, WebsocketConnectionManager connectionManager)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
            this.connectionManager = connectionManager;
        }

        public async Task Handle(WebsocketConnection websocketConnection, UpdateRoleCommand command)
        {
            IdentityRole role = await roleManager.FindByIdAsync(command.Id);

            if (role != null)
            {
                role.Name = command.Name;
                await roleManager.UpdateNormalizedRoleNameAsync(role);

                IdentityResult result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    await websocketConnection.Send(new UpdateRoleResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        NewRole = ModelHelper.GenerateRoleData(role)
                    });

                    IRealtimeAuthContext db = GetContext();

                    await MessageHelper.SendRolesUpdate(db, connectionManager);

                    if (db.UserRoles.Any(ur => ur.RoleId == role.Id))
                    {
                        dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);
                        await MessageHelper.SendUsersUpdate(db, contextTypeContainer, usermanager, connectionManager);
                    }
                }
                else
                {
                    await websocketConnection.Send(new UpdateRoleResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        IdentityErrors = result.Errors
                    });
                }
            }
            else
            {
                await websocketConnection.SendException<UpdateRoleResponse>(command, "Role not found");
            }            
        }
    }
}
