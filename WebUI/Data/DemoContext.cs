using System;
using System.Collections.Generic;
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
    }
}
