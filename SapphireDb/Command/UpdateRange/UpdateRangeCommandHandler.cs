using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Command.CreateRange;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

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

        public async Task<ResponseBase> Handle(HttpInformation context, UpdateRangeCommand command)
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
                    return command.CreateExceptionResponse<UpdateRangeResponse>(ex);
                }
            }

            return command.CreateExceptionResponse<UpdateRangeResponse>(new CollectionNotFoundException());
        }

        private async Task<ResponseBase> InitializeUpdate(UpdateRangeCommand command,
            KeyValuePair<Type, string> property,
            HttpInformation context, SapphireDbContext db)
        {
            List<object> originalUpdatedValues = command.Entries.Select(e => e.Value)
                .Select(value => value.ToObject(property.Key))
                .ToList();

            UpdateRangeResponse response;
            bool updateRejected;

            do
            {
                updateRejected = false;

                response = new UpdateRangeResponse
                {
                    ReferenceId = command.ReferenceId,
                    Results = originalUpdatedValues.Select((originalValue, index) =>
                    {
                        if (!property.Key.CanUpdate(context, originalValue, serviceProvider))
                        {
                            return (UpdateResponse) command.CreateExceptionResponse<UpdateResponse>(
                                new UnauthorizedException("The user is not authorized for this action."));
                        }

                        object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, originalValue);
                        object value = db.Find(property.Key, primaryKeys);

                        if (value != null)
                        {
                            return ApplyChangesToDb(property, value, originalValue,
                                command.Entries[index].UpdatedProperties, db, context);
                        }

                        return (ValidatedResponseBase) CreateRangeCommandHandler
                            .SetPropertiesAndValidate<UpdateEventAttribute>(db, property, value, context,
                                serviceProvider);
                    }).ToList()
                };

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    foreach (EntityEntry entityEntry in db.ChangeTracker.Entries())
                    {
                        await entityEntry.ReloadAsync();
                    }

                    updateRejected = true;
                }
            } while (updateRejected);

            foreach (object value in originalUpdatedValues)
            {
                property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.After,
                    value, context, serviceProvider);
            }

            return response;
        }

        private UpdateResponse ApplyChangesToDb(KeyValuePair<Type, string> property, object dbValue, object originalValue,
            JObject updatedProperties, SapphireDbContext db, HttpInformation context)
        {
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before,
                dbValue, context, serviceProvider);

            List<string> mergeErrors = null;

            if (dbValue is SapphireOfflineEntity dbValueOfflineEntity &&
                originalValue is SapphireOfflineEntity originalOfflineEntity &&
                !dbValueOfflineEntity.ModifiedOn.EqualWithTolerance(originalOfflineEntity.ModifiedOn, db.Database.ProviderName))
            {
                if (property.Key.GetModelAttributesInfo().DisableAutoMergeAttribute == null)
                {
                    mergeErrors = property.Key.MergeFields(dbValueOfflineEntity, originalOfflineEntity,
                        updatedProperties, context, serviceProvider);
                }
                else
                {
                    return new UpdateResponse()
                    {
                        Value = originalValue,
                        UpdatedProperties = updatedProperties,
                        Error = new SapphireDbError(
                            new OperationRejectedException("Update rejected. The object state has changed"))
                    };
                }
            }
            else
            {
                property.Key.UpdateFields(dbValue, updatedProperties, context, serviceProvider);
            }

            if (!ValidationHelper.ValidateModel(dbValue, serviceProvider,
                out Dictionary<string, List<string>> validationResults))
            {
                return new UpdateResponse()
                {
                    Value = dbValue,
                    UpdatedProperties = updatedProperties,
                    ValidationResults = validationResults
                };
            }

            db.Update(dbValue);

            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.BeforeSave,
                dbValue, context, serviceProvider);

            return new UpdateResponse()
            {
                Value = dbValue,
                UpdatedProperties = updatedProperties,
                ValidationResults = mergeErrors != null && mergeErrors.Any()
                    ? mergeErrors.ToDictionary(v => v, v => new List<string>() {"merge conflict"})
                    : null
            };
        }
    }
}