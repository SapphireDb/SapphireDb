using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class PublishCommandHandler : CommandHandlerBase, ICommandHandler<PublishCommand>
    {
        private IServiceProvider serviceProvider;

        public PublishCommandHandler(DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection, IServiceProvider _serviceProvider)
            : base(dbContextAccesor, websocketConnection)
        {
            serviceProvider = _serviceProvider;
        }

        public Task Handle(PublishCommand command)
        {
            RealtimeMessageSender messageSender = (RealtimeMessageSender)serviceProvider.GetService(typeof(RealtimeMessageSender));
            messageSender.Publish(command.Topic, command.Data);

            return Task.CompletedTask;
        }
    }
}
