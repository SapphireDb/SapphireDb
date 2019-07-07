using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UpdateCommandHandler : CommandHandlerBase, ICommandHandler<UpdateCommand>, IRestFallback
    {
        private readonly IServiceProvider serviceProvider;

        public UpdateCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpContext context, UpdateCommand command)
        {
            RealtimeDbContext db = GetContext();
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
            HttpContext context, RealtimeDbContext db)
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
            RealtimeDbContext db, HttpContext context, UpdateCommand command)
        {
            property.Key.UpdateFields(value, updateValue, db, context, serviceProvider);

            MethodInfo beforeMethodInfo = property.Key.GetMethod("BeforeUpdate",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (beforeMethodInfo != null && beforeMethodInfo.ReturnType == typeof(void))
            {
                beforeMethodInfo.Invoke(value, beforeMethodInfo.CreateParameters(context, serviceProvider));
            }

            if (!ValidationHelper.ValidateModel(value, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new UpdateResponse()
                {
                    UpdatedObject = value,
                    ReferenceId = command.ReferenceId,
                    ValidationResults = validationResults
                };
            }

            db.SaveChanges();

            MethodInfo afterMethodInfo = property.Key.GetMethod("AfterUpdate",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (afterMethodInfo != null && afterMethodInfo.ReturnType == typeof(void))
            {
                afterMethodInfo.Invoke(value, afterMethodInfo.CreateParameters(context, serviceProvider));
            }

            return new UpdateResponse()
            {
                UpdatedObject = value,
                ReferenceId = command.ReferenceId
            };
        }
    }
}
