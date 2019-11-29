using System;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.SubscribeUsers
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

        public async Task<ResponseBase> Handle(HttpInformation context, SubscribeUsersCommand command)
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
