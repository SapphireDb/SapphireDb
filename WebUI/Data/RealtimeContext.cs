using Microsoft.EntityFrameworkCore;
using WebUI.Data.Models;
using SapphireDb;

namespace WebUI.Data
{
    //Derive Context from SapphireDbContext
    public class RealtimeContext : SapphireDbContext
    {
        public RealtimeContext(DbContextOptions<RealtimeContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Test> Tests { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Log> Logs { get; set; }

        public DbSet<Config> Configs { get; set; }
    }
}
