using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase;
using WebUI.Data.Models;

namespace WebUI.Data
{
    public class SecondRealtimeContext : RealtimeContext
    {
        public SecondRealtimeContext(DbContextOptions<RealtimeContext> options, RealtimeDatabaseNotifier notifier) : base(options, notifier) {}

        public DbSet<Test> Tests { get; set; }
    }
}
