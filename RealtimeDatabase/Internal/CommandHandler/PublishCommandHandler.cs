using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

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

        public Task<ResponseBase> Handle(HttpContext context, PublishCommand command)
        {
            messageSender.Publish(command.Topic, command.Data);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
