using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SapphireDb.Helper;

namespace SapphireDb.Command.Subscribe
{
    public class ChangeResponse : ResponseBase
    {
        public ChangeResponse(EntityEntry change, SapphireDbContext db)
        {
            Value = change.Entity;
            PrimaryValues = Value.GetType().GetPrimaryKeyValues(db, Value);

            CollectionName = ((SapphireDbContext)change.Context).GetType().GetDbSetTypes()[change.Metadata.ClrType].ToLowerInvariant();

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

        public ChangeResponse() {}

        public ChangeResponse CreateResponse(string referenceId, object value)
        {
            return new ChangeResponse()
            {
                State = State,
                Value = value,
                ReferenceId = referenceId,
                CollectionName = CollectionName,
                PrimaryValues = PrimaryValues,
                Error = Error
            };
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
