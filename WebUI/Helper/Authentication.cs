using FileContextCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebUI.Data;
using WebUI.Data.Authentication;

namespace WebUI.Helper
{
    public static class Authentication
    {
        public static void ConfigureJWTAuthService(this IServiceCollection services, IConfigurationSection jwtOptions)
        {
            services.AddDbContext<AuthenticationDbContext>(options => options.UseFileContext(databasename: "auth"));

            services.AddIdentity<AppUser, IdentityRole>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 2;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<AuthenticationDbContext>();

            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions[nameof(JWTOptions.SecretKey)]));

            services.Configure<JWTOptions>(cfg =>
            {
                cfg.Issuer = jwtOptions[nameof(JWTOptions.Issuer)];
                cfg.Audience = jwtOptions[nameof(JWTOptions.Audience)];
                cfg.SecretKey = jwtOptions[nameof(JWTOptions.SecretKey)];
                cfg.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions[nameof(JWTOptions.Issuer)],

                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(cfg => {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg => {
                cfg.TokenValidationParameters = tokenValidationParameters;
                cfg.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        return Task.CompletedTask;
                    },
                    OnMessageReceived = ctx =>
                    {
                        // When using JWT as authentication add this to enable authentication
                        string bearer = ctx.Request.Query["bearer"];
                        if (!String.IsNullOrEmpty(bearer))
                        {
                            ctx.Token = bearer;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(cfg =>
            {

            });
        }
    }
}
