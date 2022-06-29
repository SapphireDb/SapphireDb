using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Command.CreateRange;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.UpdateRange
{
    class UpdateRangeCommandHandler : CommandHandlerBase, ICommandHandler<UpdateRangeCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly SapphireDatabaseOptions _databaseOptions;

        public UpdateRangeCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider, SapphireDatabaseOptions databaseOptions)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
            _databaseOptions = databaseOptions;
        }

        public async Task<ResponseBase> Handle(IConnectionInformation context, UpdateRangeCommand command,
            ExecutionContext executionContext)
        {
            DbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key == null)
            {
                throw new CollectionNotFoundException(command.ContextName, command.CollectionName);
            }

            if (property.Key.GetModelAttributesInfo().DisableUpdateAttribute != null ||
                _databaseOptions.OnlyIncludedEntities && property.Key.GetModelAttributesInfo().IncludeEntityAttribute == null)
            {
                throw new OperationDisabledException("Update", command.ContextName, command.CollectionName);
            }
            
            return await InitializeUpdate(command, property, context, db);
        }

        private async Task<ResponseBase> InitializeUpdate(UpdateRangeCommand command,
            KeyValuePair<Type, string> property,
            IConnectionInformation context, DbContext db)
        {
            bool updateRejected;

            List<ValidatedResponseBase> updateResults;

            do
            {
                updateRejected = false;

                updateResults = command.Entries.Select<UpdateEntry, ValidatedResponseBase>((updateEntry) =>
                {
                    object[] primaryKeys = property.Key.GetPrimaryKeyValuesFromJson(db, updateEntry.Value);
                    object dbValue = db.Find(property.Key, primaryKeys);

                    if (dbValue == null)
                    {
                        if (property.Key.JsonContainsData(db, updateEntry.Value))
                        {
                            updateEntry.Value.Merge(updateEntry.UpdatedProperties);
                            object completeValue = updateEntry.Value.ToObject(property.Key, serviceProvider);

                            return CreateRangeCommandHandler.SetPropertiesAndValidate<UpdateEventAttribute>(db, property, completeValue, context, serviceProvider);
                        }

                        throw new ValueNotFoundException(command.ContextName, command.CollectionName, primaryKeys);
                    }

                    if (!property.Key.CanUpdate(context, dbValue, serviceProvider, updateEntry.UpdatedProperties))
                    {
                        throw new UnauthorizedException("The user is not authorized for this action.");
                    }

                    return ApplyChangesToDb(command, property, dbValue, updateEntry.Value, updateEntry.UpdatedProperties, db, context);
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

            foreach (ValidatedResponseBase response in updateResults)
            {
                if (response.Value != null)
                {
                    property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.After,
                        response.Value, null, context, serviceProvider, db);   
                }
            }

            return new UpdateRangeResponse
            {
                ReferenceId = command.ReferenceId,
                Results = updateResults
            };
        }

        private UpdateResponse ApplyChangesToDb(UpdateRangeCommand command, KeyValuePair<Type, string> property, object dbValue,
            JObject originalValue,
            JObject updatedProperties, DbContext db, IConnectionInformation context)
        {
            int insteadOfExecuteCount = property.Key.ExecuteHookMethods<UpdateEventAttribute>(
                ModelStoreEventAttributeBase.EventType.InsteadOf,
                dbValue, null, context, serviceProvider, db);

            if (insteadOfExecuteCount > 0)
            {
                return new UpdateResponse();
            }
            
            property.Key.ExecuteHookMethods<UpdateEventAttribute>(ModelStoreEventAttributeBase.EventType.Before,
                dbValue, updatedProperties, context, serviceProvider, db);

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
                dbValue, updatedProperties, context, serviceProvider, db);

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