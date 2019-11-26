using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RealtimeDatabase.Command.Subscribe;

namespace RealtimeDatabase
{
    public class RealtimeDbContext : DbContext
    {
        private readonly RealtimeDatabaseNotifier notifier;
        public readonly Dictionary<Type, string> sets;

        public RealtimeDbContext(DbContextOptions options, RealtimeDatabaseNotifier notifier) : base(options)
        {
            this.notifier = notifier;

            sets = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToDictionary(p => p.PropertyType.GenericTypeArguments.FirstOrDefault(), p => p.Name);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            List<ChangeResponse> changes = GetChanges();
            int result = base.SaveChanges(acceptAllChangesOnSuccess);
            notifier.HandleChanges(changes, GetType());
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<ChangeResponse> changes = GetChanges();
            int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            notifier.HandleChanges(changes, GetType());
            return result;
        }

        private List<ChangeResponse> GetChanges()
        {
            return ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified)
                .Select(e => new ChangeResponse(e, this)).ToList();
        }
    }
}
