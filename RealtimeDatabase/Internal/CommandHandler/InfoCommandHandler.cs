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
        public InfoCommandHandler(DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection)
            : base(dbContextAccesor, websocketConnection)
        {

        }

        public async Task Handle(InfoCommand command)
        {
            RealtimeDbContext db = GetContext();

            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                string[] primaryKeys = property.Key.GetPrimaryKeyNames(db);

                InfoResponse infoResponse;

                RealtimeAuthorizeAttribute authorizeAttribute = property.Key.GetCustomAttribute<RealtimeAuthorizeAttribute>();

                if (authorizeAttribute != null)
                {
                    infoResponse = new InfoResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        PrimaryKeys = primaryKeys,
                        RolesDelete = authorizeAttribute.RolesDelete,
                        RolesRead = authorizeAttribute.RolesRead,
                        RolesWrite = authorizeAttribute.RolesWrite,
                        OnlyAuthorized = true
                    };
                }
                else
                {
                    infoResponse = new InfoResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        PrimaryKeys = primaryKeys
                    };
                }

                await websocketConnection.Websocket.Send(infoResponse);
            }
        }
    }
}
