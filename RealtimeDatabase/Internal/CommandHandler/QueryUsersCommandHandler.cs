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
    class QueryUsersCommandHandler : AuthCommandHandlerBase, ICommandHandler<QueryUsersCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public QueryUsersCommandHandler(AuthDbContextAccesor authDbContextAccesor, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task Handle(WebsocketConnection websocketConnection, QueryUsersCommand command)
        {
            IRealtimeAuthContext context = GetContext();
            object usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            IEnumerable<IdentityUser> users = ((IQueryable<IdentityUser>)contextTypeContainer
                .UserManagerType.GetProperty("Users").GetValue(usermanager));

            IEnumerable<IdentityRole> roles = context.Roles;
            IEnumerable<IdentityUserRole<string>> userRoles = context.UserRoles;

            List<Dictionary<string, object>> usersConverted = users
                .Select(u =>
                {
                    Dictionary<string, object> userData = ModelHelper.GenerateUserData(u);
                    userData["Roles"] = userRoles.Where(ur => ur.UserId == u.Id)
                        .Select(ur => roles.FirstOrDefault(r => r.Id == ur.RoleId)?.Name).ToArray();
                    return userData;
                }).ToList();

            await SendMessage(websocketConnection, new QueryUsersResponse()
            {
                ReferenceId = command.ReferenceId,
                Users = usersConverted
            });
        }
    }
}
