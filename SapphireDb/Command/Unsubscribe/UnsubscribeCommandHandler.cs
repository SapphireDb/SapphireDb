using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Unsubscribe
{
    class UnsubscribeCommandHandler : CommandHandlerBase, ICommandHandler<UnsubscribeCommand>, INeedsConnection
    {
        private readonly SubscriptionManager subscriptionManager;
        public ConnectionBase Connection { get; set; }

        public UnsubscribeCommandHandler(DbContextAccesor dbContextAccessor, SubscriptionManager subscriptionManager)
            : base(dbContextAccessor)
        {
            this.subscriptionManager = subscriptionManager;
        }

        public Task<ResponseBase> Handle(HttpInformation context, UnsubscribeCommand command)
        {
            subscriptionManager.RemoveSubscription(command.ReferenceId);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
