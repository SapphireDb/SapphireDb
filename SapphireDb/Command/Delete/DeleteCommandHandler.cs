using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Delete
{
    class DeleteCommandHandler : CommandHandlerBase, ICommandHandler<DeleteCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public DeleteCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpInformation context, DeleteCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key != null)
            {
                try
                {
                    object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, command.Value);
                    object value = db.Find(property.Key, primaryKeys);
                    
                    if (value != null)
                    {
                        if (value is SapphireOfflineEntity valueOfflineEntity &&
                            command.Value.TryGetValue("modifiedOn", out JValue modifiedOn))
                        {
                            DateTime commandModifiedOn = modifiedOn.ToObject<DateTime>();

                            if (valueOfflineEntity.ModifiedOn.Round(TimeSpan.FromMilliseconds(1))
                                != commandModifiedOn.Round(TimeSpan.FromMilliseconds(1)))
                            {
                                return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>(
                                    "Deletion rejected. The object state has changed."));
                            }
                        }
                        
                        if (!property.Key.CanRemove(context, value, serviceProvider))
                        {
                            return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>(
                                "The user is not authorized for this action."));
                        }
                        
                        property.Key.ExecuteHookMethods<RemoveEventAttribute>(ModelStoreEventAttributeBase.EventType.Before, value, context, serviceProvider);

                        db.Remove(value);

                        property.Key.ExecuteHookMethods<RemoveEventAttribute>(ModelStoreEventAttributeBase.EventType.BeforeSave, value, context, serviceProvider);

                        db.SaveChanges();

                        property.Key.ExecuteHookMethods<RemoveEventAttribute>(ModelStoreEventAttributeBase.EventType.After, value, context, serviceProvider);

                        return Task.FromResult<ResponseBase>(new DeleteResponse()
                        {
                            ReferenceId = command.ReferenceId
                        });
                    }

                    return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>("The value was not found."));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>(ex));
                }
            }

            return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>("No set for collection was found."));
        }
    }
}
