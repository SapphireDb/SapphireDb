using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class MessageCommandHandler : CommandHandlerBase, ICommandHandler<MessageCommand>
    {
        private readonly RealtimeMessageSender messageSender;
        private readonly IServiceProvider serviceProvider;

        public MessageCommandHandler(DbContextAccesor dbContextAccesor, RealtimeMessageSender messageSender, IServiceProvider serviceProvider)
            : base(dbContextAccesor)
        {
            this.messageSender = messageSender;
            this.serviceProvider = serviceProvider;
        }

        public async Task Handle(WebsocketConnection websocketConnection, MessageCommand command)
        {
            await messageSender.Send(command.Data);
        }
    }
}
