using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account;

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
            HandleChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            HandleChanges();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void HandleChanges()
        {
            List<ChangeResponse> changes = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified)
                .Select(e => new ChangeResponse(e, this)).ToList();

            _ = notifier.HandleChanges(changes);
        }
    }
}
