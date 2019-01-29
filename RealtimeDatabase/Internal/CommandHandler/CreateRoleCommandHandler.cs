using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CreateRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<CreateRoleCommand>, IRestFallback
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly WebsocketConnectionManager connectionManager;

        public CreateRoleCommandHandler(AuthDbContextAccesor authDbContextAccessor,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, WebsocketConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.roleManager = roleManager;
            this.connectionManager = connectionManager;
        }

        public async Task<ResponseBase> Handle(HttpContext context, CreateRoleCommand command)
        {
            IdentityRole newRole = new IdentityRole(command.Name);
            IdentityResult result = await roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                MessageHelper.SendRolesUpdate(GetContext(), connectionManager);

                return new CreateRoleResponse()
                {
                    ReferenceId = command.ReferenceId,
                    NewRole = ModelHelper.GenerateRoleData(newRole)
                };
            }
            else
            {
                return new CreateRoleResponse()
                {
                    ReferenceId = command.ReferenceId,
                    IdentityErrors = result.Errors
                };
            }
        }
    }
}
