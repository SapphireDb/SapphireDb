using RealtimeDatabase.Models.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
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
