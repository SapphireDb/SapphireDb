using RealtimeDatabase.Models.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
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
