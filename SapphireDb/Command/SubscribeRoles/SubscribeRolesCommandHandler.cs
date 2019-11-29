using System;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.SubscribeRoles
{
    class SubscribeRolesCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeRolesCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public SubscribeRolesCommandHandler(AuthDbContextAccesor authDbContextAccessor, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
        }

        public async Task<ResponseBase> Handle(HttpInformation context, SubscribeRolesCommand command)
        {
            await Connection.AddRolesSubscription(command);

            return new SubscribeRolesResponse()
            {
                ReferenceId = command.ReferenceId,
                Roles = ModelHelper.GetRoles(GetContext()).ToList()
            };
        }
    }
}
