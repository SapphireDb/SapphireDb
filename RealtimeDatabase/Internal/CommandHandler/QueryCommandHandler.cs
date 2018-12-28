using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class QueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryCommand>
    {
        public QueryCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public Task Handle(WebsocketConnection websocketConnection, QueryCommand command)
        {
            ModelHelper.GetAndSendCollectionSet(GetContext(), command, websocketConnection);
            return Task.CompletedTask;
        }
    }
}
