using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.CreateRole
{
    class CreateRoleCommandHandler : AuthCommandHandlerBase, ICommandHandler<CreateRoleCommand>
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ConnectionManager connectionManager;

        public CreateRoleCommandHandler(AuthDbContextAccesor authDbContextAccessor,
            IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, ConnectionManager connectionManager)
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
