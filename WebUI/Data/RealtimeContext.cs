using Microsoft.EntityFrameworkCore;
using WebUI.Data.Models;
using RealtimeDatabase;

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

        public DbSet<Log> Logs { get; set; }
    }
}
