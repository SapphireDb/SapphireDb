using RealtimeDatabase.Models.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UnsubscribeCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public UnsubscribeCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public async Task<ResponseBase> Handle(HttpContext context, UnsubscribeCommand command)
        {
            await Connection.RemoveSubscription(command);
            return null;
        }
    }
}
