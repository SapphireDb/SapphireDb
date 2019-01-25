using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class MessageCommandHandler : CommandHandlerBase, ICommandHandler<MessageCommand>
    {
        private readonly RealtimeMessageSender messageSender;

        public MessageCommandHandler(DbContextAccesor dbContextAccessor, RealtimeMessageSender messageSender)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
        }

        public async Task Handle(WebsocketConnection websocketConnection, MessageCommand command)
        {
            await messageSender.Send(command.Data);
        }
    }
}
