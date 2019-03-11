using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Models.Auth
{
    public class JwtOptions
    {
        public JwtOptions(string secretKey, string issuer, string audience = null, int validFor = 60)
        {
            SecretKey = secretKey;
            Issuer = issuer;
            Audience = audience;
            ValidFor = TimeSpan.FromMinutes(validFor);
        }

        public JwtOptions(IConfigurationSection configuration)
        {
            SecretKey = configuration[nameof(SecretKey)];
            Issuer = configuration[nameof(Issuer)];
            Audience = configuration[nameof(Audience)];
            ValidFor = TimeSpan.FromMinutes(configuration.GetValue<int>(nameof(ValidFor)));
        }

        private string _Issuer;
        public string Issuer {
            get => _Issuer;
            set
            {
                _Issuer = value;
                SetTokenValidationParameters();
            }
        }

        public string Audience { get; set; }

        private string _SecretKey;
        public string SecretKey {
            get => _SecretKey;
            set
            {
                _SecretKey = value;
                SigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(value));
                SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
                SetTokenValidationParameters();
            }
        }

        public SymmetricSecurityKey SigningKey { get; private set; }

        public SigningCredentials SigningCredentials { get; private set; }

        public TokenValidationParameters TokenValidationParameters { get; private set; }

        public Func<Task<string>> JtiGenerator =>
          () => Task.FromResult(Guid.NewGuid().ToString());

        public DateTime NotBefore => DateTime.UtcNow;

        public DateTime IssuedAt => DateTime.UtcNow;

        public TimeSpan ValidFor { get; set; }

        public DateTime Expiration => IssuedAt.Add(ValidFor);

        private void SetTokenValidationParameters()
        {
            TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,

                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SigningKey,

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
