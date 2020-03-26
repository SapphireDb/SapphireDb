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
        private readonly MessageSubscriptionManager subscriptionManager;

        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor, MessageSubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, SubscribeMessageCommand command)
        {
            if (!MessageTopicHelper.IsAllowedForSubscribe(command.Topic, context))
            {
                return Task.FromResult(command.CreateExceptionResponse<ResponseBase>("Not allowed to subscribe this topic"));
            }

            subscriptionManager.AddSubscription(command.Topic, command.ReferenceId, Connection);

            if (SapphireMessageSender.RetainedTopicMessages.TryGetValue(command.Topic, out object retainedMessage))
            {
                _ = Connection.Send(new TopicResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Message = retainedMessage
                });
            }

            return Task.FromResult<ResponseBase>(null);
        }
    }
}
