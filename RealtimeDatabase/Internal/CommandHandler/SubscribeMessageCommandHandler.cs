using RealtimeDatabase.Models.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeMessageCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private RealtimeDatabaseOptions options;

        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor, RealtimeDatabaseOptions options)
            : base(dbContextAccessor)
        {
            this.options = options;
        }

        public async Task<ResponseBase> Handle(HttpContext context, SubscribeMessageCommand command)
        {
            if (!options.IsAllowedForTopicSubscribe(context, command.Topic))
            {
                return command.CreateExceptionResponse<ResponseBase>("Not allowed to subscribe this topic");
            }

            await Connection.AddMessageSubscription(command);

            return null;
        }
    }
}
