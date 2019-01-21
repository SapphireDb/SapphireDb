using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class QueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryCommand>
    {
        public QueryCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {
        }

        public async Task Handle(WebsocketConnection websocketConnection, QueryCommand command)
        {
            await MessageHelper.SendCollection(GetContext(), command, websocketConnection);
        }
    }
}
