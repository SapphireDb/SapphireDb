using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Command.Update;
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

        public Task<ResponseBase> Handle(HttpInformation context, UpdateRangeCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key != null)
            {
                try
                {
                    return Task.FromResult(InitializeUpdate(command, property, context, db));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(command.CreateExceptionResponse<UpdateRangeResponse>(ex));
                }
            }

            return Task.FromResult(command.CreateExceptionResponse<UpdateRangeResponse>("No set for collection was found."));
        }

        private ResponseBase InitializeUpdate(UpdateRangeCommand command, KeyValuePair<Type, string> property,
            HttpInformation context, SapphireDbContext db)
        {
            object[] updateValues = command.UpdateValues.Values<JObject>().Select(newValue => newValue.ToObject(property.Key)).ToArray();

            UpdateRangeResponse response = new UpdateRangeResponse
            {
                ReferenceId = command.ReferenceId,
                Results = updateValues.Select(updateValue =>
                {
                    if (!property.Key.CanUpdate(context, updateValue, serviceProvider))
                    {
                        return command.CreateExceptionResponse<UpdateResponse>(
                            "The user is not authorized for this action.");
                    }

                    object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, updateValue);
                    object value = db.Find(property.Key, primaryKeys);
                    
                    if (value != null)
                    {
                        db.Entry(value).State = EntityState.Detached;
                        return ApplyChangesToDb(property, value, updateValue, db, context);
                    }

                    return command.CreateExceptionResponse<UpdateResponse>("No value to update was found");
                }).ToList()
            };


            db.SaveChanges();

            foreach (object value in updateValues)
            {
                property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.After, value, context, serviceProvider);
            }

            return response;
        }

        private ResponseBase ApplyChangesToDb(KeyValuePair<Type, string> property, object value, object updateValue, SapphireDbContext db, HttpInformation context)
        {
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before, value, context, serviceProvider);
            
            property.Key.UpdateFields(value, updateValue, db, context, serviceProvider);

            if (!ValidationHelper.ValidateModel(value, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new UpdateResponse()
                {
                    UpdatedObject = value,
                    ValidationResults = validationResults
                };
            }
            
            db.Update(value);

            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.BeforeSave, value, context, serviceProvider);
            
            return new UpdateResponse()
            {
                UpdatedObject = value
            };
        }
    }
}
