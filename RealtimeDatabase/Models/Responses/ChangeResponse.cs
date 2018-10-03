using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    public class ChangeResponse : ResponseBase
    {
        public ChangeResponse(EntityEntry change, RealtimeDbContext db)
        {
            Value = change.Entity;
            PrimaryValues = Value.GetType().GetPrimaryKeyValues(db, Value);

            CollectionName = ((RealtimeDbContext)change.Context).sets[change.Metadata.ClrType].ToLowerInvariant();

            switch (change.State)
            {
                case EntityState.Added:
                    State = ChangeState.Added;
                    break;
                case EntityState.Deleted:
                    State = ChangeState.Deleted;
                    break;
                case EntityState.Modified:
                    State = ChangeState.Modified;
                    break;
            }
        }

        public ChangeState State { get; set; }

        public object Value { get; set; }

        public object[] PrimaryValues { get; set; }

        public string CollectionName { get; set; }

        public enum ChangeState
        {
            Added, Deleted, Modified
        }
    }
}
