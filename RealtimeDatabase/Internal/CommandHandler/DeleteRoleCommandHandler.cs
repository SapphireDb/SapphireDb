using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class DeleteRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<DeleteRoleCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly WebsocketConnectionManager connectionManager;

        public DeleteRoleCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, WebsocketConnectionManager connectionManager)
            : base(authDbContextAccesor, serviceProvider)
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
                    IRealtimeAuthContext db = GetContext();
                    db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.RoleId == role.Id));
                    db.SaveChanges();

                    await SendMessage(websocketConnection, new DeleteRoleResponse()
                    {
                        ReferenceId = command.ReferenceId
                    });

                    await MessageHelper.SendRolesUpdate(db, connectionManager);
                    
                    dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);
                    await MessageHelper.SendUsersUpdate(db, contextTypeContainer, usermanager, connectionManager);
                }
                else
                {
                    await SendMessage(websocketConnection, new DeleteRoleResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        IdentityErrors = result.Errors
                    });
                }
            }
            else
            {
                await SendMessage(websocketConnection, new DeleteRoleResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("Role not found")
                });
            }            
        }
    }
}
