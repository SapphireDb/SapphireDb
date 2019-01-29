using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UpdateRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<UpdateRoleCommand>, IRestFallback
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly WebsocketConnectionManager connectionManager;

        public UpdateRoleCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, WebsocketConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
            this.connectionManager = connectionManager;
        }

        public async Task<ResponseBase> Handle(HttpContext context, UpdateRoleCommand command)
        {
            IdentityRole role = await roleManager.FindByIdAsync(command.Id);

            if (role != null)
            {
                role.Name = command.Name;
                await roleManager.UpdateNormalizedRoleNameAsync(role);

                IdentityResult result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return SendDataToClient(command, role);
                }
                else
                {
                    return new UpdateRoleResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        IdentityErrors = result.Errors
                    };
                }
            }
            else
            {
                return command.CreateExceptionResponse<UpdateRoleResponse>("Role not found");
            }            
        }

        private ResponseBase SendDataToClient(UpdateRoleCommand command, IdentityRole role)
        {
            IRealtimeAuthContext db = GetContext();

            MessageHelper.SendRolesUpdate(db, connectionManager);

            if (db.UserRoles.Any(ur => ur.RoleId == role.Id))
            {
                dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);
                MessageHelper.SendUsersUpdate(db, contextTypeContainer, usermanager, connectionManager);
            }

            return new UpdateRoleResponse()
            {
                ReferenceId = command.ReferenceId,
                NewRole = ModelHelper.GenerateRoleData(role)
            };
        }
    }
}
