using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebUI.Data.Authentication;

namespace WebUI
{
    public class JwtIssuer
    {
        private readonly JwtOptions jwtOptions;
        private readonly UserManager<AppUser> userManager;

        public JwtIssuer(JwtOptions jwtOptions, UserManager<AppUser> userManager)
        {
            this.jwtOptions = jwtOptions;
            this.userManager = userManager;
        }

        public async Task<string> GenerateEncodedToken(AppUser user)
        {
            List<Claim> claims = await CreateClaims(user);

            IList<string> roles = await userManager.GetRolesAsync(user);

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
            
            return claims;
        }
    }
}
