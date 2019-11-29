using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SapphireDb;
using WebUI.Data.Models;

namespace WebUI.Data
{
    public class SecondRealtimeContext : SapphireDbContext
    {
        public SecondRealtimeContext(DbContextOptions<SecondRealtimeContext> options, SapphireDatabaseNotifier notifier) : base(options, notifier) {}

        public DbSet<Test> Tests { get; set; }
    }
}
