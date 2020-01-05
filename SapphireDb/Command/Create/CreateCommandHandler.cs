﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Attributes;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Create
{
    class CreateCommandHandler : CommandHandlerBase, ICommandHandler<CreateCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public CreateCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider) 
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpInformation context, CreateCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

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

        private ResponseBase CreateObject(CreateCommand command, KeyValuePair<Type, string> property, HttpInformation context, SapphireDbContext db)
        {
            object newValue = command.Value.ToObject(property.Key);

            if (!property.Key.CanCreate(context, newValue, serviceProvider))
            {
                    return command.CreateExceptionResponse<CreateResponse>(
                        "The user is not authorized for this action.");
            }

            return SetPropertiesAndValidate(db, property, newValue, context, command);
        }

        private ResponseBase SetPropertiesAndValidate(SapphireDbContext db, KeyValuePair<Type, string> property, object newValue, HttpInformation context,
            CreateCommand command)
        {
            object newEntityObject = property.Key.SetFields(newValue, db);

            if (!ValidationHelper.ValidateModel(newEntityObject, serviceProvider, out Dictionary<string, List<string>> validationResults))
            {
                return new CreateResponse()
                {
                    NewObject = newEntityObject,
                    ReferenceId = command.ReferenceId,
                    ValidationResults = validationResults
                };
            }

            db.Add(newEntityObject);

            property.Key.ExecuteHookMethods<CreateEventAttribute>(v => v.Before, newEntityObject, context, serviceProvider);
            property.Key.ExecuteHookMethods<CreateEventAttribute>(v => v.BeforeSave, newEntityObject, context, serviceProvider);

            db.SaveChanges();

            property.Key.ExecuteHookMethods<CreateEventAttribute>(v => v.After, newEntityObject, context, serviceProvider);

            return new CreateResponse()
            {
                NewObject = newEntityObject,
                ReferenceId = command.ReferenceId
            };
        }
    }
}
