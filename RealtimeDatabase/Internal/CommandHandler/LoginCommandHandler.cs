using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class LoginCommandHandler : AuthCommandHandlerBase, ICommandHandler<LoginCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly JwtOptions jwtOptions;
        private readonly JwtIssuer jwtIssuer;

        public LoginCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer, JwtOptions jwtOptions, JwtIssuer jwtIssuer, IServiceProvider serviceProvider)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.jwtOptions = jwtOptions;
            this.jwtIssuer = jwtIssuer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, LoginCommand command)
        {
            if (String.IsNullOrEmpty(command.Username) || String.IsNullOrEmpty(command.Password))
            {
                await SendMessage(websocketConnection, new LoginResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("Username and password cannot be empty")
                });
                return;
            }

            object usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            IdentityUser userToVerify =
                await (dynamic)contextTypeContainer.UserManagerType.GetMethod("FindByNameAsync").Invoke(usermanager, new object[] { command.Username }) ??
                await (dynamic)contextTypeContainer.UserManagerType.GetMethod("FindByEmailAsync").Invoke(usermanager, new object[] { command.Username });

            if (userToVerify != null)
            {
                if ((bool)await (dynamic)contextTypeContainer.UserManagerType.GetMethod("CheckPasswordAsync").Invoke(usermanager, new object[] { userToVerify, command.Password }))
                {
                    RefreshToken rT = new RefreshToken()
                    {
                        UserId = userToVerify.Id
                    };

                    IRealtimeAuthContext context = GetContext();
                    context.RefreshTokens.RemoveRange(context.RefreshTokens.Where(rt => rt.CreatedOn.Add(jwtOptions.ValidFor) < DateTime.UtcNow));
                    context.RefreshTokens.Add(rT);
                    context.SaveChanges();

                    LoginResponse loginResponse = new LoginResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        AuthToken = await jwtIssuer.GenerateEncodedToken(userToVerify),
                        ExpiresAt = jwtOptions.Expiration,
                        ValidFor = jwtOptions.ValidFor.TotalSeconds,
                        RefreshToken = rT.RefreshKey
                    };

                    loginResponse.GenerateUserData(userToVerify);
                    loginResponse.UserData["Roles"] =
                        await (dynamic)contextTypeContainer.UserManagerType.GetMethod("GetRolesAsync").Invoke(usermanager, new object[] { userToVerify });


                    await SendMessage(websocketConnection, loginResponse);
                    return;
                }
            }

            await SendMessage(websocketConnection, new LoginResponse()
            {
                ReferenceId = command.ReferenceId,
                Error = new Exception("Login failed")
            });
        }
    }
}
