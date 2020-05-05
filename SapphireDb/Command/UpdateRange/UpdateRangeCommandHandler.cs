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
            bool updateRejected;

            List<Tuple<ValidatedResponseBase, object>> updateResults;

            do
            {
                updateRejected = false;

                updateResults = command.Entries.Select((updateEntry, index) =>
                {
                    object[] primaryKeys = property.Key.GetPrimaryKeyValuesFromJson(db, updateEntry.Value);
                    object dbValue = db.Find(property.Key, primaryKeys);

                    if (dbValue == null)
                    {
                        if (property.Key.JsonContainsData(db, updateEntry.Value))
                        {
                            updateEntry.Value.Merge(updateEntry.UpdatedProperties);
                            object completeValue = updateEntry.Value.ToObject(property.Key);

                            return new Tuple<ValidatedResponseBase, object>(CreateRangeCommandHandler
                                .SetPropertiesAndValidate<UpdateEventAttribute>(db, property, completeValue, context,
                                    serviceProvider), completeValue);
                        }

                        return new Tuple<ValidatedResponseBase, object>(
                            (UpdateResponse) command.CreateExceptionResponse<UpdateResponse>(
                                new OperationRejectedException("Update failed. The object was not found")), null);
                    }

                    if (!property.Key.CanUpdate(context, dbValue, serviceProvider))
                    {
                        return new Tuple<ValidatedResponseBase, object>(
                            (UpdateResponse) command.CreateExceptionResponse<UpdateResponse>(
                                new UnauthorizedException("The user is not authorized for this action.")), null);
                    }

                    return new Tuple<ValidatedResponseBase, object>(ApplyChangesToDb(property, dbValue,
                        updateEntry.Value,
                        updateEntry.UpdatedProperties, db, context), dbValue);
                }).ToList();

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

            foreach (Tuple<ValidatedResponseBase, object> value in updateResults)
            {
                if (value.Item2 != null)
                {
                    property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.After,
                        value.Item2, context, serviceProvider);
                }
            }

            return new UpdateRangeResponse
            {
                ReferenceId = command.ReferenceId,
                Results = updateResults.Select(r => r.Item1).ToList()
            };
        }

        private UpdateResponse ApplyChangesToDb(KeyValuePair<Type, string> property, object dbValue,
            JObject originalValue,
            JObject updatedProperties, SapphireDbContext db, HttpInformation context)
        {
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before,
                dbValue, context, serviceProvider);

            List<Tuple<string, string>> mergeErrors = null;

            DateTime? modifiedOn = originalValue.GetTimestamp();

            if (dbValue is SapphireOfflineEntity dbValueOfflineEntity &&
                property.Key.JsonContainsData(db, originalValue) &&
                modifiedOn.HasValue &&
                !dbValueOfflineEntity.ModifiedOn.EqualWithTolerance(modifiedOn.Value, db.Database.ProviderName))
            {
                if (property.Key.GetModelAttributesInfo().DisableAutoMergeAttribute == null)
                {
                    mergeErrors = property.Key.MergeFields(dbValueOfflineEntity, originalValue,
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
                property.Key.UpdateFields(dbValue, originalValue, updatedProperties, context, serviceProvider);
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
                    ? mergeErrors.ToDictionary(v => v.Item1, v => new List<string>() {$"merge conflict: {v.Item2}"})
                    : null
            };
        }
    }
}