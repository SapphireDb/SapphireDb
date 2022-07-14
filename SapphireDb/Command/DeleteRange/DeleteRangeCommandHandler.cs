using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.DeleteRange
{
    class DeleteRangeCommandHandler : CommandHandlerBase, ICommandHandler<DeleteRangeCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly SapphireDatabaseOptions _databaseOptions;

        public DeleteRangeCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider,
            SapphireDatabaseOptions databaseOptions)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
            _databaseOptions = databaseOptions;
        }

        public async Task<ResponseBase> Handle(IConnectionInformation context, DeleteRangeCommand command,
            ExecutionContext executionContext)
        {
            DbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key == null)
            {
                throw new CollectionNotFoundException(command.ContextName, command.CollectionName);
            }
            
            if (property.Key.GetModelAttributesInfo().DisableDeleteAttribute != null ||
                _databaseOptions.OnlyIncludedEntities && property.Key.GetModelAttributesInfo().IncludeEntityAttribute == null)
            {
                throw new OperationDisabledException("Delete", command.ContextName, command.CollectionName);
            }
            
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

                        if (value == null)
                        {
                            throw new ValueNotFoundException(command.ContextName, command.CollectionName, primaryKeys);
                        }
                        
                        if (value is SapphireOfflineEntity valueOfflineEntity &&
                            valuePrimaryKeys.TryGetValue("modifiedOn", out JValue modifiedOn))
                        {
                            DateTimeOffset commandModifiedOn = modifiedOn.ToObject<DateTimeOffset>();

                            if (!valueOfflineEntity.ModifiedOn.EqualWithTolerance(commandModifiedOn,
                                db.Database.ProviderName))
                            {
                                throw new DeleteRejectedException(command.ContextName, command.CollectionName, primaryKeys);
                            }
                        }

                        if (!property.Key.CanDelete(context, value, serviceProvider))
                        {
                            throw new UnauthorizedException("The user is not authorized for this action");
                        }

                        int insteadOfExecuteCount = property.Key.ExecuteHookMethods<DeleteEventAttribute>(
                            ModelStoreEventAttributeBase.EventType.InsteadOf,
                            value, null, context, serviceProvider, db, out object insteadOfResult);

                        if (insteadOfExecuteCount > 0)
                        {
                            return new DeleteResponse()
                            {
                                Value = insteadOfResult
                            };
                        }
                        
                        property.Key.ExecuteHookMethods<DeleteEventAttribute>(
                            ModelStoreEventAttributeBase.EventType.Before, value, null, context, serviceProvider, db, out _);

                        db.Remove(value);

                        property.Key.ExecuteHookMethods<DeleteEventAttribute>(
                            ModelStoreEventAttributeBase.EventType.BeforeSave, value, null, context,
                            serviceProvider, db, out _);

                        removedValues.Add(value);

                        return new DeleteResponse()
                        {
                            Value = value,
                            ReferenceId = command.ReferenceId
                        };

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
                property.Key.ExecuteHookMethods<DeleteEventAttribute>(
                    ModelStoreEventAttributeBase.EventType.After, value, null, context, serviceProvider, db, out _);
            }

            return response;
        }
    }
}