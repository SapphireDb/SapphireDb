using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Publish
{
    class PublishCommandHandler : CommandHandlerBase, ICommandHandler<PublishCommand>
    {
        private readonly SapphireMessageSender messageSender;
        private readonly SapphireDatabaseOptions options;

        public PublishCommandHandler(DbContextAccesor dbContextAccessor, SapphireMessageSender messageSender, SapphireDatabaseOptions options)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
            this.options = options;
        }

        public Task<ResponseBase> Handle(HttpInformation context, PublishCommand command)
        {
            if (!options.IsAllowedForTopicPublish(context, command.Topic))
            {
                return Task.FromResult(
                    command.CreateExceptionResponse<ResponseBase>("User is not allowed to publish data to this topic"));
            }

            messageSender.Publish(command.Topic, command.Data, command.Retain);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
