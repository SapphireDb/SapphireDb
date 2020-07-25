using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.SubscribeMessage
{
    class SubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeMessageCommand>,
        INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private readonly MessageSubscriptionManager subscriptionManager;

        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor,
            MessageSubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, SubscribeMessageCommand command,
            ExecutionContext executionContext)
        {
            if (!MessageTopicHelper.IsAllowedForSubscribe(command.Topic, context))
            {
                throw new UnauthorizedException("Not allowed to subscribe this topic(s)");
            }

            subscriptionManager.AddSubscription(command.Topic, command.ReferenceId, Connection);

            SapphireMessageSender.RetainedTopicMessages
                .Where(m => m.Key.MatchesGlobPattern(command.Topic))
                .ToList()
                .ForEach((retainedMessage) =>
                {
                    _ = Connection.Send(new TopicResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        Message = retainedMessage.Value,
                        Topic = retainedMessage.Key
                    });
                });
            
            return Task.FromResult<ResponseBase>(null);
        }
    }
}