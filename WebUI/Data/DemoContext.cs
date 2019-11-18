using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase;
using WebUI.Data.DemoDb;

namespace WebUI.Data
{
    public class DemoContext : RealtimeDbContext
    {
        public DemoContext(DbContextOptions<DemoContext> options, RealtimeDatabaseNotifier notifier) : base(options, notifier)
        {
        }

        public DbSet<DemoEntry> Entries { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Document> Documents { get; set; }
    }
}
