using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CreateCommandHandler : CommandHandlerBase, ICommandHandler<CreateCommand>, IRestFallback
    {
        private readonly IServiceProvider serviceProvider;

        public CreateCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider) 
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpContext context, CreateCommand command)
        {
            RealtimeDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                try
                {
                    return Task.FromResult(CreateObject(command, property, context, db));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(command.CreateExceptionResponse<CreateResponse>(ex));
                }
            }

            return Task.FromResult(command.CreateExceptionResponse<CreateResponse>("No set for collection was found."));
        }

        private ResponseBase CreateObject(CreateCommand command, KeyValuePair<Type, string> property, HttpContext context, RealtimeDbContext db)
        {
            object newValue = command.Value.ToObject(property.Key);

            if (!property.Key.CanCreate(context, newValue, serviceProvider))
            {
                    return command.CreateExceptionResponse<CreateResponse>(
                        "The user is not authorized for this action.");
            }

            return SetPropertiesAndValidate(db, property, newValue, context, command);
        }

        private ResponseBase SetPropertiesAndValidate(RealtimeDbContext db, KeyValuePair<Type, string> property, object newValue, HttpContext context,
            CreateCommand command)
        {
            property.Key.ExecuteHookMethod<CreateEventAttribute>(v => v.Before, newValue, context, serviceProvider);

            if (!ValidationHelper.ValidateModel(newValue, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new CreateResponse()
                {
                    NewObject = newValue,
                    ReferenceId = command.ReferenceId,
                    ValidationResults = validationResults
                };
            }

            db.Add(newValue);

            property.Key.ExecuteHookMethod<CreateEventAttribute>(v => v.BeforeSave, newValue, context, serviceProvider);

            db.SaveChanges();

            property.Key.ExecuteHookMethod<CreateEventAttribute>(v => v.After, newValue, context, serviceProvider);

            return new CreateResponse()
            {
                NewObject = newValue,
                ReferenceId = command.ReferenceId
            };
        }
    }
}
