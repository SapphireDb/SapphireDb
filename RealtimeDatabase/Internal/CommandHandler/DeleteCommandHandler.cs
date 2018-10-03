using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class DeleteCommandHandler : CommandHandlerBase, ICommandHandler<DeleteCommand>
    {
        public DeleteCommandHandler(DbContextAccesor contextAccesor, WebsocketConnection websocketConnection)
            : base(contextAccesor, websocketConnection)
        {

        }

        public async Task Handle(DeleteCommand command)
        {
            RealtimeDbContext db = GetContext();

            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                if (!property.Key.IsAuthorized(websocketConnection, RealtimeAuthorizeAttribute.OperationType.Delete))
                {
                    await websocketConnection.Websocket.Send(new DeleteResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        Error = new Exception("The user is not authorized for this action.")
                    });

                    return;
                }

                try
                {
                    object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, command.PrimaryKeys);
                    object value = db.Find(property.Key, primaryKeys);

                    if (value != null)
                    {
                        db.Remove(value);
                        db.SaveChanges();

                        await websocketConnection.Websocket.Send(new DeleteResponse()
                        {
                            ReferenceId = command.ReferenceId
                        });
                    }
                }
                catch (Exception ex)
                {
                    await websocketConnection.Websocket.Send(new DeleteResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        Error = ex
                    });
                }
            }
        }
    }
}
