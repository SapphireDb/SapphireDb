using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class PublishCommandHandler : CommandHandlerBase, ICommandHandler<PublishCommand>, IRestFallback
    {
        private readonly RealtimeMessageSender messageSender;
        private readonly RealtimeDatabaseOptions options;

        public PublishCommandHandler(DbContextAccesor dbContextAccessor, RealtimeMessageSender messageSender, RealtimeDatabaseOptions options)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
            this.options = options;
        }

        public Task<ResponseBase> Handle(HttpContext context, PublishCommand command)
        {
            if (!options.IsAllowedForTopicPublish(context, command.Topic))
            {
                return Task.FromResult(
                    command.CreateExceptionResponse<ResponseBase>("User is not allowed to publish data to this topic"));
            }

            messageSender.Publish(command.Topic, command.Data);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
