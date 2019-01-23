using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class PublishCommandHandler : CommandHandlerBase, ICommandHandler<PublishCommand>
    {
        private readonly RealtimeMessageSender messageSender;

        public PublishCommandHandler(DbContextAccesor dbContextAccesor, RealtimeMessageSender messageSender)
            : base(dbContextAccesor)
        {
            this.messageSender = messageSender;
        }

        public async Task Handle(WebsocketConnection websocketConnection, PublishCommand command)
        {
            await messageSender.Publish(command.Topic, command.Data);
        }
    }
}
