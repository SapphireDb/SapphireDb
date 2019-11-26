using System.Threading.Tasks;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.UnsubscribeMessage
{
    class UnsubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeMessageCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public UnsubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpInformation context, UnsubscribeMessageCommand command)
        {
            await Connection.RemoveMessageSubscription(command);
            return null;
        }
    }
}
