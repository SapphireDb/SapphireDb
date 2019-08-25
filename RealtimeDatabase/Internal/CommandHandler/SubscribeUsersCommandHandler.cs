using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeUsersCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeUsersCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private readonly AuthDbContextTypeContainer contextTypeContainer;

        public SubscribeUsersCommandHandler(AuthDbContextAccesor authDbContextAccessor, AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
        }

        public async Task<ResponseBase> Handle(HttpContext context, SubscribeUsersCommand command)
        {
            await Connection.AddUsersSubscription(command);

            object usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            return new SubscribeUsersResponse()
            {
                ReferenceId = command.ReferenceId,
                Users = ModelHelper.GetUsers(GetContext(), contextTypeContainer, usermanager).ToList()
            };
        }
    }
}
