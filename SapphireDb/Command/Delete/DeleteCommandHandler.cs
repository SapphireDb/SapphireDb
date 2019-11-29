using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Attributes;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Delete
{
    class DeleteCommandHandler : CommandHandlerBase, ICommandHandler<DeleteCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public DeleteCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpInformation context, DeleteCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
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
