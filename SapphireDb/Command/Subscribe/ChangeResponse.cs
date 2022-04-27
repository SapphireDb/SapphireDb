using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Command.Subscribe
{
    public class ChangesResponse : ResponseBase
    {
        public List<ChangeResponse> Changes { get; set; }
    }
    
    public class ChangeResponse : ResponseBase
    {
        public ChangeResponse(EntityEntry change)
        {
            Value = change.Entity;
            
            Dictionary<Type, string> dbSetTypes = change.Context.GetType().GetDbSetTypes();
            if (!dbSetTypes.ContainsKey(change.Metadata.ClrType) || change.Metadata.ClrType == typeof(SapphireDbFile))
            {
                ValidChange = false;
            }
            else
            {
                CollectionName = dbSetTypes[change.Metadata.ClrType].ToLowerInvariant();
            }

            switch (change.State)
            {
                case EntityState.Added:
                    State = ChangeState.Added;
                    break;
                case EntityState.Deleted:
                    State = ChangeState.Deleted;
                    break;
                case EntityState.Modified:
                    OriginalValue = change.OriginalValues.ToObject();
                    State = ChangeState.Modified;
                    break;
            }
        }

        public ChangeResponse() {}

        public ChangeResponse CreateResponse(string referenceId, object value)
        {
            return new ChangeResponse()
            {
                State = State,
                Value = value,
                ReferenceId = referenceId,
                CollectionName = CollectionName,
                Error = Error
            };
        }

        [JsonIgnore]
        public bool ValidChange { get; } = true;
        
        public ChangeState State { get; set; }

        public object Value { get; set; }
        
        public object OriginalValue { get; set; }

        public string CollectionName { get; set; }

        public enum ChangeState
        {
            Added, Deleted, Modified
        }
    }
}
