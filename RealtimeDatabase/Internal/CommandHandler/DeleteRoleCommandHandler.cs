using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
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
        private readonly RoleManager<IdentityRole> roleManager;

        public DeleteRoleCommandHandler(AuthDbContextAccesor authDbContextAccesor,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.roleManager = roleManager;
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
