using RealtimeDatabase.Models.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
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
