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
        private IServiceProvider serviceProvider;

        public MessageCommandHandler(DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection, IServiceProvider _serviceProvider)
            : base(dbContextAccesor, websocketConnection)
        {
            serviceProvider = _serviceProvider;
        }

        public Task Handle(MessageCommand command)
        {
            RealtimeMessageSender messageSender = (RealtimeMessageSender)serviceProvider.GetService(typeof(RealtimeMessageSender));
            messageSender.Send(command.Data);

            return Task.CompletedTask;
        }
    }
}
