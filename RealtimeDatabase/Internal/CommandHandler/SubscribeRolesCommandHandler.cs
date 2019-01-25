using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeRolesCommandHandler : AuthCommandHandlerBase, ICommandHandler<SubscribeRolesCommand>
    {
        public SubscribeRolesCommandHandler(AuthDbContextAccesor authDbContextAccessor, IServiceProvider serviceProvider)
            : base(authDbContextAccessor, serviceProvider)
        {
        }

        public async Task Handle(WebsocketConnection websocketConnection, SubscribeRolesCommand command)
        {
            await websocketConnection.Lock.WaitAsync();

            try
            {
                websocketConnection.RolesSubscription = command.ReferenceId;
            }
            finally
            {
                websocketConnection.Lock.Release();
            }

            await websocketConnection.Send(new SubscribeRolesResponse()
            {
                ReferenceId = command.ReferenceId,
                Roles = ModelHelper.GetRoles(GetContext()).ToList()
            });
        }
    }
}
