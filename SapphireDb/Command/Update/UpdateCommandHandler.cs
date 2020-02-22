using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Attributes;
using SapphireDb.Command.Create;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Update
{
    class UpdateCommandHandler : CommandHandlerBase, ICommandHandler<UpdateCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public UpdateCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, UpdateCommand command)
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
                    return command.CreateExceptionResponse<UpdateResponse>(ex);
                }
            }

            return command.CreateExceptionResponse<UpdateResponse>("No set for collection was found.");
        }

        private async Task<ResponseBase> InitializeUpdate(UpdateCommand command, KeyValuePair<Type, string> property,
            HttpInformation context, SapphireDbContext db)
        {
            object updateValue = command.Value.ToObject(property.Key);

            if (!property.Key.CanUpdate(context, updateValue, serviceProvider))
            {
                return command.CreateExceptionResponse<UpdateResponse>("The user is not authorized for this action.");
            }

            object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, updateValue);
            object value = db.Find(property.Key, primaryKeys);

            if (value != null)
            {
                return await SaveChangesToDb(property, value, updateValue, db, context, command);
            }
            
            return CreateCommandHandler.SetPropertiesAndValidate<UpdateEventAttribute>(db, property, updateValue, context,
                command.ReferenceId, serviceProvider);
        }

        private async Task<ResponseBase> SaveChangesToDb(KeyValuePair<Type, string> property, object value, object updateValue,
            SapphireDbContext db, HttpInformation context, UpdateCommand command)
        {
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before, value, context, serviceProvider);
            
            property.Key.UpdateFields(value, updateValue, context, serviceProvider);

            if (!ValidationHelper.ValidateModel(value, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new UpdateResponse()
                {
                    Value = value,
                    ReferenceId = command.ReferenceId,
                    ValidationResults = validationResults
                };
            }

            db.Update(value);

            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.BeforeSave, value, context, serviceProvider);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await ex.Entries.Single().ReloadAsync();
                await Task.Delay(10);
                
                ResponseBase result =  CreateCommandHandler.SetPropertiesAndValidate<UpdateEventAttribute>(db, property, updateValue, context,
                    command.ReferenceId, serviceProvider);
                
                db.SaveChanges();
            
                return result;
            }
            
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.After, value, context, serviceProvider);

            return new UpdateResponse()
            {
                Value = value,
                ReferenceId = command.ReferenceId
            };
        }
    }
}
