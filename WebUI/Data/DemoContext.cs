using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SapphireDb;
using WebUI.Data.DemoDb;

namespace WebUI.Data
{
    public class DemoContext : SapphireDbContext
    {
        public DemoContext(DbContextOptions<DemoContext> options, SapphireDatabaseNotifier notifier) : base(options, notifier)
        {
        }

        public DbSet<DemoEntry> Entries { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<Pixel> Pixels { get; set; }

        public DbSet<IncludeDemoUser> Users { get; set; }
        
        public DbSet<IncludeDemoUserEntry> UserEntries { get; set; }

        public DbSet<Log> Logs { get; set; }
        
        public DbSet<EventDemo> EventDemos { get; set; }
        public DbSet<EventDemoDerived> EventDerivedDemos { get; set; }
        
        public DbSet<ValidationDemo> ValidationDemos { get; set; }
    }
}
