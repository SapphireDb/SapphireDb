using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class RenewCommandHandler : AuthCommandHandlerBase, ICommandHandler<RenewCommand>
    {
        private readonly JwtOptions jwtOptions;
        private readonly JwtIssuer jwtIssuer;
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public RenewCommandHandler(AuthDbContextAccesor authDbContextAccesor, JwtOptions jwtOptions, JwtIssuer jwtIssuer, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.jwtOptions = jwtOptions;
            this.jwtIssuer = jwtIssuer;
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, RenewCommand command)
        {
            if (String.IsNullOrEmpty(command.UserId) || String.IsNullOrEmpty(command.RefreshToken))
            {
                await websocketConnection.Send(new RenewResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("Userid and refresh token cannot be empty")
                });
                return;
            }

            IRealtimeAuthContext context = GetContext();

            if (!await QueryToken(context, command, websocketConnection))
                return;

            if (!await RenewToken(command, context, websocketConnection))
            {
                await websocketConnection.Send(new RenewResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("Renew failed")
                });
            }
        }

        private async Task<bool> QueryToken(IRealtimeAuthContext context, RenewCommand command, WebsocketConnection websocketConnection)
        {
            RefreshToken rT = GetRefreshToken(context, command);

            if (rT == null)
            {
                await websocketConnection.Send(new RenewResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("Wrong refresh token")
                });
                return false;
            }

            context.RefreshTokens.Remove(rT);

            return true;
        }

        private async Task<bool> RenewToken(RenewCommand command, IRealtimeAuthContext context, WebsocketConnection websocketConnection)
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

                await websocketConnection.Send(renewResponse);
                return true;
            }

            return false;
        }

        private RefreshToken GetRefreshToken(IRealtimeAuthContext context, RenewCommand command)
        {
            context.RefreshTokens.RemoveRange(context.RefreshTokens.Where(rt => rt.CreatedOn.Add(jwtOptions.ValidFor) < DateTime.UtcNow));
            context.SaveChanges();

            return context.RefreshTokens.FirstOrDefault(r => r.UserId == command.UserId && r.RefreshKey == command.RefreshToken);
        }
    }
}
