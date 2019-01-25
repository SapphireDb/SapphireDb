using System;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class QueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public QueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task Handle(WebsocketConnection websocketConnection, QueryCommand command)
        {
            await MessageHelper.SendCollection(GetContext(), command, websocketConnection, serviceProvider);
        }
    }
}
