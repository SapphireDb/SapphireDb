using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Message
{
    class MessageCommandHandler : CommandHandlerBase, ICommandHandler<MessageCommand>
    {
        private readonly SapphireMessageSender messageSender;
        private readonly SapphireDatabaseOptions options;

        public MessageCommandHandler(DbContextAccesor dbContextAccessor, SapphireMessageSender messageSender, SapphireDatabaseOptions options)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
            this.options = options;
        }

        public Task<ResponseBase> Handle(HttpInformation context, MessageCommand command)
        {
            if (!options.IsAllowedToSendMessages(context))
            {
                return Task.FromResult(
                    command.CreateExceptionResponse<ResponseBase>("User is not allowed to send messages"));
            }
            
            messageSender.Send(command.Data, command.Filter, command.FilterParameters);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
