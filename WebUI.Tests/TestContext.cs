using Microsoft.EntityFrameworkCore;
using WebUI.Data.Models;

namespace WebUI.Tests
{
    internal class TestContext : DbContext
    {
        public TestContext(DbContextOptions<TestContext> options) :base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        
        public DbSet<Test> Tests { get; set; }
    }
}
