using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Models.Auth;

namespace SapphireDb
{
    public interface ISapphireAuthContext
    {
        int SaveChanges();

        DbSet<RefreshToken> RefreshTokens { get; set; }

        DbSet<IdentityRole> Roles { get; set; }

        DbSet<IdentityUserRole<string>> UserRoles { get; set; }

    }
}