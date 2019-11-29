using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.DeleteUser
{
    class DeleteUserCommandHandler : AuthCommandHandlerBase, ICommandHandler<DeleteUserCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly ConnectionManager connectionManager;

        public DeleteUserCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer, 
            IServiceProvider serviceProvider, ConnectionManager connectionManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.connectionManager = connectionManager;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, DeleteUserCommand command)
        {
            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            IdentityUser user = await usermanager.FindByIdAsync(command.Id);

            if (user != null)
            {
                IdentityResult result =
                    await contextTypeContainer.UserManagerType.GetMethod("DeleteAsync").Invoke(usermanager, new object[] { user });

                if (result.Succeeded)
                {
                    ISapphireAuthContext db = GetContext();

                    db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.UserId == user.Id));
                    db.RefreshTokens.RemoveRange(db.RefreshTokens.Where(rt => rt.UserId == user.Id));
                    db.SaveChanges();

                    MessageHelper.SendUsersUpdate(db, contextTypeContainer, usermanager, connectionManager);
                    MessageHelper.SendRolesUpdate(db, connectionManager);

                    return new DeleteUserResponse()
                    {
                        ReferenceId = command.ReferenceId
                    };
                }
            }

            return command.CreateExceptionResponse<DeleteUserResponse>("Deleting user failed");
        }
    }
}
