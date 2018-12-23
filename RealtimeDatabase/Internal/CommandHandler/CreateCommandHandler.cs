using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CreateCommandHandler : CommandHandlerBase, ICommandHandler<CreateCommand>
    {
        public CreateCommandHandler(DbContextAccesor contextAccesor) 
            : base(contextAccesor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, CreateCommand command)
        {
            RealtimeDbContext db = GetContext();

            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                try
                {
                    object newValue = command.Value.ToObject(property.Key);

                    if (!property.Key.CanCreate(websocketConnection, newValue))
                    {
                        await SendMessage(websocketConnection, new CreateResponse()
                        {
                            ReferenceId = command.ReferenceId,
                            Error = new Exception("The user is not authorized for this action.")
                        });

                        return;
                    }

                    MethodInfo mi = property.Key.GetMethod("OnCreate");

                    if (mi != null && 
                        mi.ReturnType == typeof(void) && 
                        mi.GetParameters().Count() == 1 && 
                        mi.GetParameters()[0].ParameterType == typeof(WebsocketConnection))
                    {
                        mi.Invoke(newValue, new object[] { websocketConnection });
                    }

                    if (!ValidationHelper.ValidateModel(newValue, out Dictionary<string, List<string>> validationResults))
                    {
                        await SendMessage(websocketConnection, new CreateResponse()
                        {
                            NewObject = newValue,
                            ReferenceId = command.ReferenceId,
                            ValidationResults = validationResults
                        });

                        return;
                    }

                    db.Add(newValue);
                    db.SaveChanges();

                    await SendMessage(websocketConnection, new CreateResponse()
                    {
                        NewObject = newValue,
                        ReferenceId = command.ReferenceId
                    });
                }
                catch (Exception ex)
                {
                    await SendMessage(websocketConnection, new CreateResponse() {
                        NewObject = command.Value,
                        ReferenceId = command.ReferenceId,
                        Error = ex
                    });
                }
            }
        }
    }
}
