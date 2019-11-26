using System.Threading.Tasks;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.Unsubscribe
{
    class UnsubscribeCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public UnsubscribeCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpInformation context, UnsubscribeCommand command)
        {
            await Connection.RemoveSubscription(command);
            return null;
        }
    }
}
