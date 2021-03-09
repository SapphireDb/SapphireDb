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

        public async Task<ResponseBase> Handle(HttpInformation context, UpdateRangeCommand command,
            ExecutionContext executionContext)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key == null)
            {
                throw new CollectionNotFoundException(command.ContextName, command.CollectionName);
            }

            if (property.Key.GetModelAttributesInfo().DisableUpdateAttribute != null)
            {
                throw new OperationDisabledException("Update", command.ContextName, command.CollectionName);
            }
            
            return await InitializeUpdate(command, property, context, db);
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

                        throw new ValueNotFoundException(command.ContextName, command.CollectionName, primaryKeys);
                    }

                    if (!property.Key.CanUpdate(context, dbValue, serviceProvider))
                    {
                        throw new UnauthorizedException("The user is not authorized for this action.");
                    }

                    return new Tuple<ValidatedResponseBase, object>(ApplyChangesToDb(command, property, dbValue,
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
                        value.Item2, null, context, serviceProvider);
                }
            }

            return new UpdateRangeResponse
            {
                ReferenceId = command.ReferenceId,
                Results = updateResults.Select(r => r.Item1).ToList()
            };
        }

        private UpdateResponse ApplyChangesToDb(UpdateRangeCommand command, KeyValuePair<Type, string> property, object dbValue,
            JObject originalValue,
            JObject updatedProperties, SapphireDbContext db, HttpInformation context)
        {
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before,
                dbValue, updatedProperties, context, serviceProvider);

            List<Tuple<string, string>> mergeErrors = null;

            DateTimeOffset? modifiedOn = originalValue.GetTimestamp();

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
                    throw new UpdateRejectedException(command.ContextName, command.CollectionName, originalValue, updatedProperties);
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
                dbValue, updatedProperties, context, serviceProvider);

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