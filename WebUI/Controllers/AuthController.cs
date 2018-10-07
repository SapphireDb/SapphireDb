using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebUI.Data;
using WebUI.Data.Authentication;
using WebUI.Data.ViewModels.Auth;

namespace WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private UserManager<AppUser> userManager;
        private JWTOptions jwtOptions;
        private AuthenticationDbContext db;

        public AuthController(UserManager<AppUser> _userManager, IOptions<JWTOptions> _jwtOptions, AuthenticationDbContext _db)
        {
            userManager = _userManager;
            jwtOptions = _jwtOptions.Value;
            db = _db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AppUser appUser = await GetAppUser(model.Username, model.Password);

            if (appUser == null)
            {
                return BadRequest("Wrong Username or Password");
            }

            RefreshToken rT = new RefreshToken()
            {
                UserId = appUser.Id
            };

            db.RefreshTokens.RemoveRange(db.RefreshTokens.Where(rt => rt.CreatedOn.Add(jwtOptions.ValidFor) < DateTime.UtcNow));
            db.RefreshTokens.Add(rT);
            await db.SaveChangesAsync();

            return new OkObjectResult(new
            {
                id = appUser.Id,
                username = appUser.UserName,
                firstname = appUser.FirstName,
                lastname = appUser.LastName,
                email = appUser.Email,
                roles = await userManager.GetRolesAsync(appUser),
                auth_token = await GenerateEncodedToken(appUser),
                refresh_token = rT.RefreshKey,
                expires_at = jwtOptions.Expiration,
                valid_for = jwtOptions.ValidFor.TotalSeconds
            });
        }

        [HttpPost("renew")]
        public async Task<IActionResult> Renew([FromBody]RenewViewModel model)
        {
            db.RefreshTokens.RemoveRange(db.RefreshTokens.Where(rt => rt.CreatedOn.Add(jwtOptions.ValidFor) < DateTime.UtcNow));
            await db.SaveChangesAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RefreshToken rT = db.RefreshTokens.FirstOrDefault(r => r.UserId == model.UserId && r.RefreshKey == model.RefreshToken);

            if (rT == null)
            {
                return BadRequest("Wrong Refresh Token");
            }

            db.RefreshTokens.Remove(rT);

            AppUser appUser = await userManager.FindByIdAsync(model.UserId);

            RefreshToken newrT = new RefreshToken()
            {
                UserId = appUser.Id
            };

            db.RefreshTokens.Add(newrT);
            await db.SaveChangesAsync();

            return new OkObjectResult(new
            {
                auth_token = await GenerateEncodedToken(appUser),
                refresh_token = newrT.RefreshKey,
                expires_at = jwtOptions.Expiration,
                valid_for = jwtOptions.ValidFor.TotalSeconds,
                roles = await userManager.GetRolesAsync(appUser)
            });
        }

        private async Task<string> GenerateEncodedToken(AppUser appUser)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, appUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, await jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, jwtOptions.IssuedAt.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                new Claim("Id", appUser.Id),
                new Claim(ClaimTypes.Name, appUser.UserName),
                new Claim(ClaimTypes.Email, appUser.Email)
            };

            IList<string> roles = await userManager.GetRolesAsync(appUser);

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

        private async Task<AppUser> GetAppUser(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                AppUser userToVerify = await userManager.FindByNameAsync(username) ?? await userManager.FindByEmailAsync(username);

                if (userToVerify != null)
                {
                    if (await userManager.CheckPasswordAsync(userToVerify, password))
                    {
                        return userToVerify;
                    }
                }
            }

            return null;
        }
    }
}