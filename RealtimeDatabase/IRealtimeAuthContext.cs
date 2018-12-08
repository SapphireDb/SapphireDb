using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Auth;

namespace RealtimeDatabase
{
    public interface IRealtimeAuthContext
    {
        int SaveChanges();

        DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}