using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal
{
    class JwtIssuer
    {
        private readonly JwtOptions jwtOptions;
        private readonly IServiceProvider serviceProvider;
        private readonly AuthDbContextTypeContainer authDbContextTypeContainer;

        public JwtIssuer(JwtOptions _jwtOptions, IServiceProvider _serviceProvider, AuthDbContextTypeContainer _authDbContextTypeContainer)
        {
            jwtOptions = _jwtOptions;
            serviceProvider = _serviceProvider;
            authDbContextTypeContainer = _authDbContextTypeContainer;
        }

        public async Task<string> GenerateEncodedToken(IdentityUser identityUser)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, identityUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, await jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, jwtOptions.IssuedAt.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                new Claim("Id", identityUser.Id),
                new Claim(ClaimTypes.Name, identityUser.UserName),
                new Claim(ClaimTypes.Email, identityUser.Email)
            };

            object userManager = serviceProvider.GetService(authDbContextTypeContainer.UserManagerType);
            IList<string> roles = 
                await (Task<IList<string>>)authDbContextTypeContainer.UserManagerType.GetMethod("GetRolesAsync").Invoke(userManager, new object[] { identityUser });

            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            Type type = identityUser.GetType();

            foreach (PropertyInfo property in type.GetProperties().Where(p => p.GetCustomAttribute<AuthClaimInformationAttribute>() != null))
            {
                claims.Add(new Claim("RealtimeAuth." + property.Name, property.GetValue(identityUser).ToString()));
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
    }
}
