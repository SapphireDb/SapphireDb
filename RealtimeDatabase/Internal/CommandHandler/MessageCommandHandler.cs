using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class MessageCommandHandler : CommandHandlerBase, ICommandHandler<MessageCommand>, IRestFallback
    {
        private readonly RealtimeMessageSender messageSender;

        public MessageCommandHandler(DbContextAccesor dbContextAccessor, RealtimeMessageSender messageSender)
            : base(dbContextAccessor)
        {
            this.messageSender = messageSender;
        }

        public Task<ResponseBase> Handle(HttpContext context, MessageCommand command)
        {
            messageSender.Send(command.Data);
            return Task.FromResult<ResponseBase>(null);
        }
    }
}
