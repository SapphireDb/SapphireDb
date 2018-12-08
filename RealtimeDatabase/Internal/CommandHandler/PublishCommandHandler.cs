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
        private readonly IServiceProvider serviceProvider;
        private readonly RealtimeMessageSender messageSender;

        public PublishCommandHandler(DbContextAccesor dbContextAccesor, RealtimeMessageSender messageSender, IServiceProvider serviceProvider)
            : base(dbContextAccesor)
        {
            this.serviceProvider = serviceProvider;
            this.messageSender = messageSender;
        }

        public Task Handle(WebsocketConnection websocketConnection, PublishCommand command)
        {
            messageSender.Publish(command.Topic, command.Data);
            return Task.CompletedTask;
        }
    }
}
