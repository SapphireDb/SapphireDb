using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Data.Models;
using RealtimeDatabase;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace WebUI.Data
{
    //Derive Context from RealtimeDbContext
    public class RealtimeContext : RealtimeDbContext
    {
        //Add RealtimeDatabaseNotifier for DI
        public RealtimeContext(DbContextOptions<RealtimeContext> options, RealtimeDatabaseNotifier notifier) : base(options, notifier)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Test> Tests { get; set; }

        public DbSet<Message> Messages { get; set; }
    }
}
