using Microsoft.AspNetCore.Identity;
using SapphireDb.Attributes;
using SapphireDb.Models.Auth;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SapphireDb.Internal
{
    class JwtIssuer
    {
        private readonly JwtOptions jwtOptions;
        private readonly IServiceProvider serviceProvider;
        private readonly AuthDbContextTypeContainer authDbContextTypeContainer;

        public JwtIssuer(JwtOptions jwtOptions, IServiceProvider serviceProvider, AuthDbContextTypeContainer authDbContextTypeContainer)
        {
            this.jwtOptions = jwtOptions;
            this.serviceProvider = serviceProvider;
            this.authDbContextTypeContainer = authDbContextTypeContainer;
        }

        public async Task<string> GenerateEncodedToken(IdentityUser identityUser)
        {
            List<Claim> claims = await CreateClaims(identityUser);

            object userManager = serviceProvider.GetService(authDbContextTypeContainer.UserManagerType);
            IList<string> roles = 
                await (Task<IList<string>>)authDbContextTypeContainer.UserManagerType.GetMethod("GetRolesAsync").Invoke(userManager, new object[] { identityUser });

            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                notBefore: jwtOptions.NotBefore,
                expires: jwtOptions.Expiration,
                signingCredentials: jwtOptions.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private async Task<List<Claim>> CreateClaims(IdentityUser identityUser)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, identityUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, await jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, jwtOptions.IssuedAt.ToUniversalTime().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer64),
                new Claim("Id", identityUser.Id),
                new Claim(ClaimTypes.Name, identityUser.UserName),
                new Claim(ClaimTypes.Email, identityUser.Email)
            };

            Type type = identityUser.GetType();

            foreach (PropertyInfo property in type.GetProperties().Where(p => p.GetCustomAttribute<AuthClaimInformationAttribute>() != null))
            {
				object value = property.GetValue(identityUser);

                if (value != null)
                {
                    claims.Add(new Claim("RealtimeAuth." + property.Name, value.ToString()));
                }
            }

            return claims;
        }
    }
}
