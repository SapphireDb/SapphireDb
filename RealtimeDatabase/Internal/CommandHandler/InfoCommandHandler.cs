using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class InfoCommandHandler : CommandHandlerBase, ICommandHandler<InfoCommand>
    {
        public InfoCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, InfoCommand command)
        {
            RealtimeDbContext db = GetContext();

            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                InfoResponse infoResponse = property.Key.GetInfoResponse(db);
                infoResponse.ReferenceId = command.ReferenceId;
                await websocketConnection.Send(infoResponse);
            }
        }
    }
}
