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
    class UpdateRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<UpdateRoleCommand>
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public UpdateRoleCommandHandler(AuthDbContextAccesor authDbContextAccesor,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.roleManager = roleManager;
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
                    await SendMessage(websocketConnection, new UpdateRoleResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        NewRole = new Dictionary<string, object>()
                        {
                            ["Id"] = role.Id,
                            ["Name"] = role.Name,
                            ["NormalizedName"] = role.NormalizedName
                        }
                    });
                }
                else
                {
                    await SendMessage(websocketConnection, new UpdateRoleResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        IdentityErrors = result.Errors
                    });
                }
            }
            else
            {
                await SendMessage(websocketConnection, new UpdateRoleResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("Role not found")
                });
            }            
        }
    }
}
