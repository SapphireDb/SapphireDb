using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class DeleteCommandHandler : CommandHandlerBase, ICommandHandler<DeleteCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public DeleteCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task Handle(WebsocketConnection websocketConnection, DeleteCommand command)
        {
            RealtimeDbContext db = GetContext();
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                try
                {
                    object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, command.PrimaryKeys);
                    object value = db.Find(property.Key, primaryKeys);

                    if (!property.Key.CanRemove(websocketConnection, value, serviceProvider))
                    {
                        await websocketConnection.SendException<DeleteResponse>(command,
                            "The user is not authorized for this action.");
                        return;
                    }

                    if (value != null)
                    {
                        db.Remove(value);
                        db.SaveChanges();

                        await websocketConnection.Send(new DeleteResponse()
                        {
                            ReferenceId = command.ReferenceId
                        });
                    }
                }
                catch (Exception ex)
                {
                    await websocketConnection.SendException<DeleteResponse>(command, ex);
                }
            }
        }
    }
}
