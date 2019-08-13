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
    class MessageCommandHandler : CommandHandlerBase, ICommandHandler<MessageCommand>, IRestFallback
    {
        private readonly RealtimeMessageSender messageSender;
        private readonly RealtimeDatabaseOptions options;

        public MessageCommandHandler(DbContextAccesor dbContextAccessor, RealtimeMessageSender messageSender, RealtimeDatabaseOptions options)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
            this.options = options;
        }

        public Task<ResponseBase> Handle(HttpContext context, MessageCommand command)
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
