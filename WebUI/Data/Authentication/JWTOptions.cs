using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Data.Authentication
{
    public class JWTOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string SecretKey { get; set; }

        public SigningCredentials SigningCredentials { get; set; }

        public Func<Task<string>> JtiGenerator =>
          () => Task.FromResult(Guid.NewGuid().ToString());

        public DateTime NotBefore => DateTime.UtcNow;

        public DateTime IssuedAt => DateTime.UtcNow;

        public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(60);

        public DateTime Expiration => IssuedAt.Add(ValidFor);
    }
}
