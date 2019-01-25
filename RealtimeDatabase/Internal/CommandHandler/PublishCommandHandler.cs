using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class PublishCommandHandler : CommandHandlerBase, ICommandHandler<PublishCommand>
    {
        private readonly RealtimeMessageSender messageSender;

        public PublishCommandHandler(DbContextAccesor dbContextAccessor, RealtimeMessageSender messageSender)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
        }

        public async Task Handle(WebsocketConnection websocketConnection, PublishCommand command)
        {
            await messageSender.Publish(command.Topic, command.Data);
        }
    }
}
