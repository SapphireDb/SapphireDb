using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.UnsubscribeRoles
{
    class UnsubscribeRolesCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeRolesCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public UnsubscribeRolesCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpInformation context, UnsubscribeRolesCommand command)
        {
            await Connection.RemoveRolesSubscription();
            return null;
        }
    }
}
