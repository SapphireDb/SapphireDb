using System.Threading.Tasks;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.UnsubscribeUsers
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
