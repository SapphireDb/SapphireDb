using System.Threading.Tasks;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.SubscribeMessage
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

        public async Task<ResponseBase> Handle(HttpInformation context, SubscribeMessageCommand command)
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
