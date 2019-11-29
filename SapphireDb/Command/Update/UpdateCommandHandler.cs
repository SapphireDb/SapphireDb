using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Attributes;
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

        public Task<ResponseBase> Handle(HttpInformation context, UpdateCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                try
                {
                    return Task.FromResult(InitializeUpdate(command, property, context, db));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(command.CreateExceptionResponse<UpdateResponse>(ex));
                }
            }

            return Task.FromResult(command.CreateExceptionResponse<UpdateResponse>("No set for collection was found."));
        }

        private ResponseBase InitializeUpdate(UpdateCommand command, KeyValuePair<Type, string> property,
            HttpInformation context, SapphireDbContext db)
        {
            object updateValue = command.UpdateValue.ToObject(property.Key);

            if (!property.Key.CanUpdate(context, updateValue, serviceProvider))
            {
                return command.CreateExceptionResponse<UpdateResponse>("The user is not authorized for this action.");
            }

            object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, updateValue);
            object value = db.Find(property.Key, primaryKeys);

            if (value != null)
            {
                return SaveChangesToDb(property, value, updateValue, db, context, command);
            }

            return command.CreateExceptionResponse<UpdateResponse>("No value to update was found");
        }

        private ResponseBase SaveChangesToDb(KeyValuePair<Type, string> property, object value, object updateValue,
            SapphireDbContext db, HttpInformation context, UpdateCommand command)
        {
            property.Key.UpdateFields(value, updateValue, db, context, serviceProvider);

            property.Key.ExecuteHookMethod<UpdateEventAttribute>(v => v.Before, value, context, serviceProvider);

            if (!ValidationHelper.ValidateModel(value, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new UpdateResponse()
                {
                    UpdatedObject = value,
                    ReferenceId = command.ReferenceId,
                    ValidationResults = validationResults
                };
            }

            property.Key.ExecuteHookMethod<UpdateEventAttribute>(v => v.BeforeSave, value, context, serviceProvider);

            db.SaveChanges();

            property.Key.ExecuteHookMethod<UpdateEventAttribute>(v => v.After, value, context, serviceProvider);

            return new UpdateResponse()
            {
                UpdatedObject = value,
                ReferenceId = command.ReferenceId
            };
        }
    }
}
