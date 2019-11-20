using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class LoginCommandHandler : AuthCommandHandlerBase, ICommandHandler<LoginCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly JwtOptions jwtOptions;
        private readonly JwtIssuer jwtIssuer;

        public LoginCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer, JwtOptions jwtOptions, JwtIssuer jwtIssuer, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.jwtOptions = jwtOptions;
            this.jwtIssuer = jwtIssuer;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, LoginCommand command)
        {
            if (string.IsNullOrEmpty(command.Username) || string.IsNullOrEmpty(command.Password))
            {
                return command.CreateExceptionResponse<LoginResponse>("Username and password cannot be empty");
            }

            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            IdentityUser userToVerify = await usermanager.FindByNameAsync(command.Username) ??
                await usermanager.FindByEmailAsync(command.Username);

            if (userToVerify != null)
            {
                if ((bool)await contextTypeContainer.UserManagerType.GetMethod("CheckPasswordAsync").Invoke(usermanager, new object[] { userToVerify, command.Password }))
                {
                    return await CreateLoginResponse(command, CreateRefreshToken(userToVerify), userToVerify, usermanager);
                }
            }

            return command.CreateExceptionResponse<LoginResponse>("Login failed");
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
