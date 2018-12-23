using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Auth;

namespace RealtimeDatabase
{
    public interface IRealtimeAuthContext
    {
        int SaveChanges();

        DbSet<RefreshToken> RefreshTokens { get; set; }

        DbSet<IdentityRole> Roles { get; set; }

        DbSet<IdentityUserRole<string>> UserRoles { get; set; }

    }
}