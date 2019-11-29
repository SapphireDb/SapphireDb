using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.UnsubscribeUsers
{
    class UnsubscribeUsersCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeUsersCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public UnsubscribeUsersCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpInformation context, UnsubscribeUsersCommand command)
        {
            await Connection.RemoveUsersSubscription();
            return null;
        }
    }
}
