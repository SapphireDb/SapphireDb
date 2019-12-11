using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jint;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Command.Delete;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.DeleteRange
{
    class DeleteRangeCommandHandler : CommandHandlerBase, ICommandHandler<DeleteRangeCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public DeleteRangeCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpInformation context, DeleteRangeCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key != null)
            {
                try
                {
                    List<object> removedValues = new List<object>();

                    DeleteRangeResponse response = new DeleteRangeResponse
                    {
                        ReferenceId = command.ReferenceId,
                        Results = command.PrimaryKeyList.Select(valuePrimaryKeys =>
                        {
                            object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, valuePrimaryKeys);
                            object value = db.Find(property.Key, primaryKeys);

                            if (!property.Key.CanRemove(context, value, serviceProvider))
                            {
                                return command.CreateExceptionResponse<DeleteResponse>(
                                    "The user is not authorized for this action.");
                            }

                            if (value != null)
                            {
                                property.Key.ExecuteHookMethod<RemoveEventAttribute>(v => v.Before, value, context, serviceProvider);

                                db.Remove(value);

                                property.Key.ExecuteHookMethod<RemoveEventAttribute>(v => v.BeforeSave, value, context, serviceProvider);

                                removedValues.Add(value);

                                return new DeleteResponse()
                                {
                                    ReferenceId = command.ReferenceId
                                };
                            }

                            return command.CreateExceptionResponse<DeleteResponse>("The value was not found.");
                        }).ToList()
                    };

                    db.SaveChanges();

                    foreach (object value in removedValues)
                    {
                        property.Key.ExecuteHookMethod<RemoveEventAttribute>(v => v.After, value, context, serviceProvider);
                    }

                    return Task.FromResult<ResponseBase>(response);
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
