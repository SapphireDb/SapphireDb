using System;
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
        public SignalRConnection Connection { get; set; }
        private readonly MessageSubscriptionManager _subscriptionManager;
        private readonly IServiceProvider _serviceProvider;

        public SubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor,
            MessageSubscriptionManager subscriptionManager, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            _subscriptionManager = subscriptionManager;
            _serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(IConnectionInformation context, SubscribeMessageCommand command,
            ExecutionContext executionContext)
        {
            if (!MessageTopicHelper.IsAllowedForSubscribe(command.Topic, context))
            {
                throw new UnauthorizedException("Not allowed to subscribe this topic(s)");
            }

            _subscriptionManager.AddSubscription(command.Topic, command.ReferenceId, Connection);

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
                    }, _serviceProvider);
                });
            
            return Task.FromResult<ResponseBase>(null);
        }
    }
}