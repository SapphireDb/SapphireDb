using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.SubscribeMessage
{
    class SubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeMessageCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private SapphireDatabaseOptions options;

        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor, SapphireDatabaseOptions options)
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

            if (SapphireMessageSender.RetainedTopicMessages.TryGetValue(command.Topic, out object retainedMessage))
            {
                _ = Connection.Send(new TopicResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Message = retainedMessage
                });
            }
            
            return null;
        }
    }
}
