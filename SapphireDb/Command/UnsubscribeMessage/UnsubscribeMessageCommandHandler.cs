using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.UnsubscribeMessage
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
