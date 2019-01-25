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
    class CreateCommandHandler : CommandHandlerBase, ICommandHandler<CreateCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public CreateCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider) 
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task Handle(WebsocketConnection websocketConnection, CreateCommand command)
        {
            RealtimeDbContext db = GetContext();
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                try
                {
                    await CreateObject(command, property, websocketConnection, db);
                }
                catch (Exception ex)
                {
                    await websocketConnection.SendException<CreateResponse>(command, ex);
                }
            }
        }

        private async Task CreateObject(CreateCommand command, KeyValuePair<Type, string> property, WebsocketConnection websocketConnection, RealtimeDbContext db)
        {
            object newValue = command.Value.ToObject(property.Key);

            if (!property.Key.CanCreate(websocketConnection, newValue, serviceProvider))
            {
                await websocketConnection.SendException<CreateResponse>(command,
                    "The user is not authorized for this action.");
                return;
            }

            if (await SetPropertiesAndValidate(property, newValue, websocketConnection, command))
            {
                db.Add(newValue);
                db.SaveChanges();

                await websocketConnection.Send(new CreateResponse()
                {
                    NewObject = newValue,
                    ReferenceId = command.ReferenceId
                });
            }
        }

        private async Task<bool> SetPropertiesAndValidate(KeyValuePair<Type, string> property, object newValue, WebsocketConnection websocketConnection,
            CreateCommand command)
        {
            MethodInfo mi = property.Key.GetMethod("OnCreate");

            if (mi != null && mi.ReturnType == typeof(void))
            {
                mi.Invoke(newValue, mi.CreateParameters(websocketConnection, serviceProvider));
            }

            if (!ValidationHelper.ValidateModel(newValue, out Dictionary<string, List<string>> validationResults))
            {
                await websocketConnection.Send(new CreateResponse()
                {
                    NewObject = newValue,
                    ReferenceId = command.ReferenceId,
                    ValidationResults = validationResults
                });

                return false;
            }

            return true;
        }
    }
}
