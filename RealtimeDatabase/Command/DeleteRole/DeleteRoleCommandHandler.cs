using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.DeleteRole
{
    class DeleteRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<DeleteRoleCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly RealtimeConnectionManager connectionManager;

        public DeleteRoleCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, RealtimeConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
            this.connectionManager = connectionManager;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, DeleteRoleCommand command)
        {
            IdentityRole role = await roleManager.FindByIdAsync(command.Id);

            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return SendDataToClients(role, command);
                }
                else
                {
                    return new DeleteRoleResponse
                    {
                        ReferenceId = command.ReferenceId,
                        IdentityErrors = result.Errors
                    };
                }
            }
            else
            {
                return command.CreateExceptionResponse<DeleteRoleResponse>("Role not found");
            }            
        }

        private ResponseBase SendDataToClients(IdentityRole role, DeleteRoleCommand command)
        {
            IRealtimeAuthContext db = GetContext();
            db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.RoleId == role.Id));
            db.SaveChanges();

            MessageHelper.SendRolesUpdate(db, connectionManager);

            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);
            MessageHelper.SendUsersUpdate(db, contextTypeContainer, usermanager, connectionManager);

            return new DeleteRoleResponse()
            {
                ReferenceId = command.ReferenceId
            };
        }
    }
}
