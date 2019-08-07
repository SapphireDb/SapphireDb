using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase;
using WebUI.Data.Models;

namespace WebUI.Data
{
    public class SecondRealtimeContext : RealtimeDbContext
    {
        public SecondRealtimeContext(DbContextOptions<SecondRealtimeContext> options, RealtimeDatabaseNotifier notifier) : base(options, notifier) {}

        public DbSet<Test> Tests { get; set; }
    }
}
