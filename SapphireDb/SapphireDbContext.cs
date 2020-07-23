﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SapphireDb.Command.Subscribe;

namespace SapphireDb
{
    public class SapphireDbContext : DbContext
    {
        private readonly ISapphireDatabaseNotifier notifier;

        public SapphireDbContext(DbContextOptions options) : base(options)
        {
            notifier = this.GetService<ISapphireDatabaseNotifier>();
        }

        public SapphireDbContext()
        {
            notifier = this.GetService<ISapphireDatabaseNotifier>();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            List<ChangeResponse> changes = GetChanges();
            int result = base.SaveChanges(acceptAllChangesOnSuccess);
            notifier?.HandleChanges(changes, GetType());
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<ChangeResponse> changes = GetChanges();
            int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            notifier?.HandleChanges(changes, GetType());
            return result;
        }

        private List<ChangeResponse> GetChanges()
        {
            return ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified)
                .Select(e => new ChangeResponse(e))
                .ToList();
        }
    }
}
