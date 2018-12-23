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
    class CreateRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<CreateRoleCommand>
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public CreateRoleCommandHandler(AuthDbContextAccesor authDbContextAccesor,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.roleManager = roleManager;
        }

        public async Task Handle(WebsocketConnection websocketConnection, CreateRoleCommand command)
        {
            IdentityRole newRole = new IdentityRole(command.Name);
            IdentityResult result = await roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                await SendMessage(websocketConnection, new CreateRoleResponse()
                {
                    ReferenceId = command.ReferenceId,
                    NewRole = new Dictionary<string, object>()
                    {
                        ["Id"] = newRole.Id,
                        ["Name"] = newRole.Name,
                        ["NormalizedName"] = newRole.NormalizedName
                    }
                });
            }
            else
            {
                await SendMessage(websocketConnection, new CreateRoleResponse()
                {
                    ReferenceId = command.ReferenceId,
                    IdentityErrors = result.Errors
                });
            }
        }
    }
}
