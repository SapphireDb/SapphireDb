using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Command.CreateRange;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.UpdateRange
{
    class UpdateRangeCommandHandler : CommandHandlerBase, ICommandHandler<UpdateRangeCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public UpdateRangeCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, UpdateRangeCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key != null)
            {
                try
                {
                    return await InitializeUpdate(command, property, context, db);
                }
                catch (Exception ex)
                {
                    return command.CreateExceptionResponse<UpdateRangeResponse>(ex);
                }
            }

            return command.CreateExceptionResponse<UpdateRangeResponse>("No set for collection was found.");
        }

        private async Task<ResponseBase> InitializeUpdate(UpdateRangeCommand command, KeyValuePair<Type, string> property,
            HttpInformation context, SapphireDbContext db)
        {
            List<object> updateValues = command.Entries.Select(e => e.Value)
                .Select(newValue => newValue.ToObject(property.Key))
                .ToList();

            UpdateRangeResponse response = new UpdateRangeResponse
            {
                ReferenceId = command.ReferenceId,
                Results = updateValues.Select((updateValue, index) =>
                {
                    if (!property.Key.CanUpdate(context, updateValue, serviceProvider))
                    {
                        return (UpdateResponse)command.CreateExceptionResponse<UpdateResponse>(
                            "The user is not authorized for this action.");
                    }

                    object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, updateValue);
                    object value = db.Find(property.Key, primaryKeys);
                    
                    if (value != null)
                    {
                        object previousValue = command.Entries[index].Previous?.ToObject(property.Key);
                        return ApplyChangesToDb(property, value, updateValue, previousValue, db, context);
                    }

                    return (ValidatedResponseBase)CreateRangeCommandHandler.SetPropertiesAndValidate<UpdateEventAttribute>(db, property, updateValue, context,
                        serviceProvider);
                }).ToList()
            };

            try
            {
                db.SaveChanges();
                
                foreach (object value in updateValues)
                {
                    property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.After, value, context, serviceProvider);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (EntityEntry entry in db.ChangeTracker.Entries())
                {
                    await entry.ReloadAsync();
                }

                response.Results = updateValues.Select(updateValue => 
                    (ValidatedResponseBase)CreateRangeCommandHandler.SetPropertiesAndValidate<UpdateEventAttribute>(db,
                        property, updateValue, context, serviceProvider)).ToList();

                db.SaveChanges();
            }

            return response;
        }

        private UpdateResponse ApplyChangesToDb(KeyValuePair<Type, string> property, object dbValue, object updateValue,
            object previousValue, SapphireDbContext db, HttpInformation context)
        {
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before, dbValue, context, serviceProvider);

            if (dbValue is SapphireOfflineEntity dbValueOfflineEntity &&
                updateValue is SapphireOfflineEntity updateOfflineEntity &&
                previousValue != null && previousValue is SapphireOfflineEntity previousValueOfflineEntity &&
                dbValueOfflineEntity.ModifiedOn.Round(TimeSpan.FromMilliseconds(1)) !=
                previousValueOfflineEntity.ModifiedOn.Round(TimeSpan.FromMilliseconds(1)))
            {
                property.Key.MergeFields(dbValueOfflineEntity, updateOfflineEntity,
                    previousValueOfflineEntity, context, serviceProvider);
            }
            else
            {
                property.Key.UpdateFields(dbValue, updateValue, context, serviceProvider);
            }

            if (!ValidationHelper.ValidateModel(dbValue, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new UpdateResponse()
                {
                    Value = dbValue,
                    ValidationResults = validationResults
                };
            }
            
            db.Update(dbValue);

            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.BeforeSave, dbValue, context, serviceProvider);
            
            return new UpdateResponse()
            {
                Value = dbValue
            };
        }
    }
}
