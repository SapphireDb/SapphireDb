using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Connection.Websocket;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CreateRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<CreateRoleCommand>
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly RealtimeConnectionManager connectionManager;

        public CreateRoleCommandHandler(AuthDbContextAccesor authDbContextAccessor,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, RealtimeConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.roleManager = roleManager;
            this.connectionManager = connectionManager;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, CreateRoleCommand command)
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
