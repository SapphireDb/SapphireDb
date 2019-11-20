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
    class RenewCommandHandler : AuthCommandHandlerBase, ICommandHandler<RenewCommand>
    {
        private readonly JwtOptions jwtOptions;
        private readonly JwtIssuer jwtIssuer;
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public RenewCommandHandler(AuthDbContextAccesor authDbContextAccessor, JwtOptions jwtOptions, JwtIssuer jwtIssuer, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.jwtOptions = jwtOptions;
            this.jwtIssuer = jwtIssuer;
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, RenewCommand command)
        {
            if (string.IsNullOrEmpty(command.UserId) || string.IsNullOrEmpty(command.RefreshToken))
            {
                return command.CreateExceptionResponse<RenewResponse>("Userid and refresh token cannot be empty");
            }

            IRealtimeAuthContext db = GetContext();

            RefreshToken rT = GetRefreshToken(db, command);

            if (rT == null)
            {
                return command.CreateExceptionResponse<RenewResponse>("Wrong refresh token");
            }

            db.RefreshTokens.Remove(rT);

            return await RenewToken(command, db);
        }

        private async Task<ResponseBase> RenewToken(RenewCommand command, IRealtimeAuthContext context)
        {
            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            IdentityUser user = await usermanager.FindByIdAsync(command.UserId);

            if (user != null)
            {
                RefreshToken newrT = new RefreshToken()
                {
                    UserId = user.Id
                };

                context.RefreshTokens.Add(newrT);
                context.SaveChanges();

                RenewResponse renewResponse = new RenewResponse()
                {
                    ReferenceId = command.ReferenceId,
                    AuthToken = await jwtIssuer.GenerateEncodedToken(user),
                    ExpiresAt = jwtOptions.Expiration,
                    ValidFor = jwtOptions.ValidFor.TotalSeconds,
                    RefreshToken = newrT.RefreshKey,
                    UserData = await ModelHelper.GenerateUserData(user, contextTypeContainer, usermanager)
                };

                return renewResponse;
            }

            return command.CreateExceptionResponse<RenewResponse>("Renew failed");
        }

        private RefreshToken GetRefreshToken(IRealtimeAuthContext context, RenewCommand command)
        {
            context.RefreshTokens.RemoveRange(context.RefreshTokens.Where(rt => rt.CreatedOn.Add(jwtOptions.ValidFor) < DateTime.UtcNow));
            context.SaveChanges();

            return context.RefreshTokens.FirstOrDefault(r => r.UserId == command.UserId && r.RefreshKey == command.RefreshToken);
        }
    }
}
