using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
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
    class DeleteCommandHandler : CommandHandlerBase, ICommandHandler<DeleteCommand>, IRestFallback
    {
        private readonly IServiceProvider serviceProvider;

        public DeleteCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpContext context, DeleteCommand command)
        {
            RealtimeDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                try
                {
                    object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, command.PrimaryKeys);
                    object value = db.Find(property.Key, primaryKeys);

                    if (!property.Key.CanRemove(context, value, serviceProvider))
                    {
                        return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>(
                            "The user is not authorized for this action."));
                    }

                    if (value != null)
                    {
                        property.Key.ExecuteHookMethod<RemoveEventAttribute>(v => v.Before, value, context, serviceProvider);

                        db.Remove(value);

                        property.Key.ExecuteHookMethod<RemoveEventAttribute>(v => v.BeforeSave, value, context, serviceProvider);

                        db.SaveChanges();

                        property.Key.ExecuteHookMethod<RemoveEventAttribute>(v => v.After, value, context, serviceProvider);

                        return Task.FromResult<ResponseBase>(new DeleteResponse()
                        {
                            ReferenceId = command.ReferenceId
                        });
                    }

                    return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>("The value was not found."));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>(ex));
                }
            }

            return Task.FromResult(command.CreateExceptionResponse<DeleteResponse>("No set for collection was found."));
        }
    }
}
