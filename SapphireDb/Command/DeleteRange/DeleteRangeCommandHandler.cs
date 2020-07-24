using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.DeleteRange
{
    class DeleteRangeCommandHandler : CommandHandlerBase, ICommandHandler<DeleteRangeCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public DeleteRangeCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, DeleteRangeCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key != null)
            {
                List<object> removedValues = new List<object>();

                DeleteRangeResponse response;
                bool updateRejected;

                do
                {
                    updateRejected = false;

                    response = new DeleteRangeResponse
                    {
                        ReferenceId = command.ReferenceId,
                        Results = command.Values.Select(valuePrimaryKeys =>
                        {
                            object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, valuePrimaryKeys);
                            object value = db.Find(property.Key, primaryKeys);

                            if (value != null)
                            {
                                if (value is SapphireOfflineEntity valueOfflineEntity &&
                                    valuePrimaryKeys.TryGetValue("modifiedOn", out JValue modifiedOn))
                                {
                                    DateTime commandModifiedOn = modifiedOn.ToObject<DateTime>();

                                    if (!valueOfflineEntity.ModifiedOn.EqualWithTolerance(commandModifiedOn,
                                        db.Database.ProviderName))
                                    {
                                        return (DeleteResponse) command.CreateExceptionResponse<DeleteResponse>(
                                            new OperationRejectedException(
                                                "Deletion rejected. The object state has changed"));
                                    }
                                }

                                if (!property.Key.CanRemove(context, value, serviceProvider))
                                {
                                    return (DeleteResponse) command.CreateExceptionResponse<DeleteResponse>(
                                        new UnauthorizedException("The user is not authorized for this action"));
                                }

                                property.Key.ExecuteHookMethods<RemoveEventAttribute>(
                                    ModelStoreEventAttributeBase.EventType.Before, value, context, serviceProvider);

                                db.Remove(value);

                                property.Key.ExecuteHookMethods<RemoveEventAttribute>(
                                    ModelStoreEventAttributeBase.EventType.BeforeSave, value, context,
                                    serviceProvider);

                                removedValues.Add(value);

                                return new DeleteResponse()
                                {
                                    Value = value,
                                    ReferenceId = command.ReferenceId
                                };
                            }

                            return (DeleteResponse) command.CreateExceptionResponse<DeleteResponse>(
                                new ValueNotFoundException());
                        }).ToList()
                    };

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        foreach (EntityEntry entityEntry in db.ChangeTracker.Entries())
                        {
                            await entityEntry.ReloadAsync();
                        }

                        updateRejected = true;
                    }
                } while (updateRejected);

                foreach (object value in removedValues)
                {
                    property.Key.ExecuteHookMethods<RemoveEventAttribute>(
                        ModelStoreEventAttributeBase.EventType.After, value, context, serviceProvider);
                }

                return response;
            }

            throw new CollectionNotFoundException();
        }
    }
}