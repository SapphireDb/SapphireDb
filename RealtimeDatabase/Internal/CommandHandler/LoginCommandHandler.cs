using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
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
            if (string.IsNullOrEmpty(command.Username) || string.IsNullOrEmpty(command.Password))
            {
                await websocketConnection.SendException<LoginResponse>(command, "Username and password cannot be empty");
                return;
            }

            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            IdentityUser userToVerify = await usermanager.FindByNameAsync(command.Username) ??
                await usermanager.FindByEmailAsync(command.Username);

            if (userToVerify != null)
            {
                if ((bool)await (dynamic)contextTypeContainer.UserManagerType.GetMethod("CheckPasswordAsync").Invoke(usermanager, new object[] { userToVerify, command.Password }))
                {
                    await websocketConnection.Send(await CreateLoginResponse(command, CreateRefreshToken(userToVerify), userToVerify, usermanager));
                    return;
                }
            }

            await websocketConnection.SendException<LoginResponse>(command, "Login failed");
        }

        private RefreshToken CreateRefreshToken(IdentityUser userToVerify)
        {
            RefreshToken rT = new RefreshToken
            {
                UserId = userToVerify.Id
            };

            IRealtimeAuthContext context = GetContext();
            context.RefreshTokens.RemoveRange(context.RefreshTokens.Where(rt => rt.CreatedOn.Add(jwtOptions.ValidFor) < DateTime.UtcNow));
            context.RefreshTokens.Add(rT);
            context.SaveChanges();

            return rT;
        }

        private async Task<LoginResponse> CreateLoginResponse(LoginCommand command, RefreshToken rT, IdentityUser userToVerify, dynamic usermanager)
        {
            return new LoginResponse
            {
                ReferenceId = command.ReferenceId,
                AuthToken = await jwtIssuer.GenerateEncodedToken(userToVerify),
                ExpiresAt = jwtOptions.Expiration,
                ValidFor = jwtOptions.ValidFor.TotalSeconds,
                RefreshToken = rT.RefreshKey,
                UserData = await ModelHelper.GenerateUserData(userToVerify, contextTypeContainer, usermanager)
            };
        }
    }
}
