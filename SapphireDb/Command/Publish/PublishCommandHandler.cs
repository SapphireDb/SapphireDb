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

        public PublishCommandHandler(DbContextAccesor dbContextAccessor, SapphireMessageSender messageSender)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
        }

        public Task<ResponseBase> Handle(HttpInformation context, PublishCommand command)
        {
            if (!MessageTopicHelper.IsAllowedForPublish(command.Topic, context))
            {
                return Task.FromResult(
                    command.CreateExceptionResponse<ResponseBase>("User is not allowed to publish data to this topic"));
            }

            messageSender.Publish(command.Topic, command.Data, command.Retain);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
