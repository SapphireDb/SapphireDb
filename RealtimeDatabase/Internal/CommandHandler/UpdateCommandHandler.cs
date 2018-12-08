using RealtimeDatabase.Models.Commands;
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
    class UpdateCommandHandler : CommandHandlerBase, ICommandHandler<UpdateCommand>
    {
        public UpdateCommandHandler(DbContextAccesor contextAccesor)
            : base(contextAccesor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, UpdateCommand command)
        {
            RealtimeDbContext db = GetContext();

            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                if (!property.Key.CanUpdate(websocketConnection))
                {
                    await SendMessage(websocketConnection, new UpdateResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        Error = new Exception("The user is not authorized for this action.")
                    });

                    return;
                }

                object updateValue = command.UpdateValue.ToObject(property.Key);

                try
                {
                    object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, updateValue);
                    object value = db.Find(property.Key, primaryKeys);

                    if (value != null)
                    {
                        property.Key.UpdateFields(value, updateValue, db, websocketConnection);

                        if (!ValidationHelper.ValidateModel(value, out Dictionary<string, List<string>> validationResults))
                        {
                            await SendMessage(websocketConnection, new UpdateResponse()
                            {
                                UpdatedObject = value,
                                ReferenceId = command.ReferenceId,
                                ValidationResults = validationResults
                            });

                            return;
                        }

                        db.SaveChanges();

                        await SendMessage(websocketConnection, new UpdateResponse()
                        {
                            UpdatedObject = value,
                            ReferenceId = command.ReferenceId
                        });
                    }
                }
                catch (Exception ex)
                {
                    await SendMessage(websocketConnection, new UpdateResponse()
                    {
                        UpdatedObject = updateValue,
                        ReferenceId = command.ReferenceId,
                        Error = ex
                    });
                }
            }
        }
    }
}
