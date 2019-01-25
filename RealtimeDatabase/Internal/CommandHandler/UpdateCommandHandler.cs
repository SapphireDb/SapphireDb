using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UpdateCommandHandler : CommandHandlerBase, ICommandHandler<UpdateCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public UpdateCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task Handle(WebsocketConnection websocketConnection, UpdateCommand command)
        {
            RealtimeDbContext db = GetContext();
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                try
                {
                    await InitializeUpdate(command, property, websocketConnection, db);
                }
                catch (Exception ex)
                {
                    await websocketConnection.SendException<UpdateResponse>(command, ex);
                }
            }
        }

        private async Task InitializeUpdate(UpdateCommand command, KeyValuePair<Type, string> property,
            WebsocketConnection websocketConnection,
            RealtimeDbContext db)
        {
            object updateValue = command.UpdateValue.ToObject(property.Key);

            if (!property.Key.CanUpdate(websocketConnection, updateValue, serviceProvider))
            {
                await websocketConnection.SendException<UpdateResponse>(command,
                    "The user is not authorized for this action.");
                return;
            }

            object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, updateValue);
            object value = db.Find(property.Key, primaryKeys);

            if (value != null)
            {
                await SaveChangesToDb(property, value, updateValue, db, websocketConnection, command);
            }
        }

        private async Task SaveChangesToDb(KeyValuePair<Type, string> property, object value, object updateValue,
            RealtimeDbContext db,
            WebsocketConnection websocketConnection, UpdateCommand command)
        {
            property.Key.UpdateFields(value, updateValue, db, websocketConnection, serviceProvider);

            MethodInfo mi = property.Key.GetMethod("OnUpdate");

            if (mi != null && mi.ReturnType == typeof(void))
            {
                mi.Invoke(value, mi.CreateParameters(websocketConnection, serviceProvider));
            }

            if (!ValidationHelper.ValidateModel(value, out Dictionary<string, List<string>> validationResults))
            {
                await websocketConnection.Send(new UpdateResponse()
                {
                    UpdatedObject = value,
                    ReferenceId = command.ReferenceId,
                    ValidationResults = validationResults
                });
                return;
            }

            db.SaveChanges();

            await websocketConnection.Send(new UpdateResponse()
            {
                UpdatedObject = value,
                ReferenceId = command.ReferenceId
            });
        }
    }
}
