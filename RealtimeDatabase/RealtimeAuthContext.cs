using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase
{
    public class RealtimeAuthContext<UserType> : IdentityDbContext<UserType>, IRealtimeAuthContext where UserType : IdentityUser
    {
        public RealtimeAuthContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
