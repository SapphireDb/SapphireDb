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
            List<object> updateValues = command.Values.Values<JObject>()
                .Select(newValue => newValue.ToObject(property.Key))
                .ToList();

            UpdateRangeResponse response = new UpdateRangeResponse
            {
                ReferenceId = command.ReferenceId,
                Results = updateValues.Select(updateValue =>
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
                        return ApplyChangesToDb(property, value, updateValue, db, context);
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
            catch (DbUpdateConcurrencyException ex)
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

        private UpdateResponse ApplyChangesToDb(KeyValuePair<Type, string> property, object value, object updateValue, SapphireDbContext db, HttpInformation context)
        {
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before, value, context, serviceProvider);
            
            property.Key.UpdateFields(value, updateValue, context, serviceProvider);

            if (!ValidationHelper.ValidateModel(value, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new UpdateResponse()
                {
                    Value = value,
                    ValidationResults = validationResults
                };
            }
            
            db.Update(value);

            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.BeforeSave, value, context, serviceProvider);
            
            return new UpdateResponse()
            {
                Value = value
            };
        }
    }
}
