using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Data.Authentication;

namespace WebUI.Data
{
    public class AuthenticationDbContext : IdentityDbContext<AppUser>
    {
        public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
        {

        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
