using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.UnsubscribeMessage
{
    class UnsubscribeMessageCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeMessageCommand>,
        INeedsConnection
    {
        private readonly MessageSubscriptionManager subscriptionManager;
        public ConnectionBase Connection { get; set; }

        public UnsubscribeMessageCommandHandler(DbContextAccesor dbContextAccessor,
            MessageSubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, UnsubscribeMessageCommand command)
        {
            subscriptionManager.RemoveSubscription(command.ReferenceId);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}