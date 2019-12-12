using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Models.Auth;

namespace SapphireDb
{
    public class SapphireAuthContext<TUserType> : IdentityDbContext<TUserType>, ISapphireAuthContext where TUserType : IdentityUser
    {
        public SapphireAuthContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
