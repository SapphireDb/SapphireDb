using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    class QueryRolesCommandHandler : AuthCommandHandlerBase, ICommandHandler<QueryRolesCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public QueryRolesCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, QueryRolesCommand command)
        {
            IRealtimeAuthContext context = GetContext();
            IEnumerable<IdentityRole> roles = context.Roles;
            IEnumerable<IdentityUserRole<string>> userRoles = context.UserRoles;

            List<Dictionary<string, object>> rolesConverted = roles
                .Select(r =>
                {
                Dictionary<string, object> roleData = new Dictionary<string, object>
                {
                        ["Id"] = r.Id,
                        ["Name"] = r.Name,
                        ["NormalizedName"] = r.NormalizedName,
                        ["UserIds"] = userRoles.Where(ur => ur.RoleId == r.Id).Select(ur => ur.UserId)
                    };
                    return roleData;
                }).ToList();

            await SendMessage(websocketConnection, new QueryRolesReponse()
            {
                ReferenceId = command.ReferenceId,
                Roles = rolesConverted
            });
        }
    }
}
