using DemoServerApplication.Data.Models;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoServerApplication.Data
{
    public class RealtimeContext : RealtimeDbContext
    {
        public RealtimeContext(DbContextOptions<RealtimeContext> options, RealtimeDatabaseNotifier notifier) : base(options, notifier)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
