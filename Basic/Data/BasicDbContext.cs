using Basic.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Basic.Data;

public class BasicDbContext : DbContext
{
    public BasicDbContext(DbContextOptions<BasicDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Message> Messages { get; set; }
}