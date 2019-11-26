using System.Threading.Tasks;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.Message
{
    class MessageCommandHandler : CommandHandlerBase, ICommandHandler<MessageCommand>
    {
        private readonly RealtimeMessageSender messageSender;
        private readonly RealtimeDatabaseOptions options;

        public MessageCommandHandler(DbContextAccesor dbContextAccessor, RealtimeMessageSender messageSender, RealtimeDatabaseOptions options)
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


            messageSender.Send(command.Data);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
