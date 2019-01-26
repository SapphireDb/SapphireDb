using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class DeleteCommandHandler : CommandHandlerBase, ICommandHandler<DeleteCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public DeleteCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpContext context, DeleteCommand command)
        {
            RealtimeDbContext db = GetContext();
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
                        db.Remove(value);
                        db.SaveChanges();

                        return Task.FromResult<ResponseBase>(new DeleteResponse()
                        {
                            ReferenceId = command.ReferenceId
                        });
                    }
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
