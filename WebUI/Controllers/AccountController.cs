using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Data.Authentication;
using WebUI.Data.ViewModels.Account;

namespace WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly JwtIssuer issuer;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, JwtIssuer issuer)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.issuer = issuer;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            AppUser user = await userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                if (await userManager.CheckPasswordAsync(user, model.Password))
                {
                    return Ok(await issuer.GenerateEncodedToken(user));
                }
            }

            return BadRequest();
        }
        
        // [HttpPost("test")]
        // public async Task<IActionResult> Post([FromBody]NewAppUserViewModel model)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //
        //     AppUser userIdentity = new AppUser()
        //     {
        //         FirstName = model.FirstName,
        //         LastName = model.LastName,
        //         Email = model.Email,
        //         UserName = model.Email
        //     };
        //
        //     IdentityResult result = await userManager.CreateAsync(userIdentity, model.Password);
        //
        //     if (!result.Succeeded)
        //     {
        //         return new BadRequestObjectResult(result.Errors);
        //     }
        //
        //     return Ok();
        // }

        //[HttpPost("claim")]
        //public async Task<IActionResult> AddClaim(string username, string type, string claim)
        //{
        //    AppUser appUser = await userManager.FindByNameAsync(username);

        //    if (appUser != null)
        //    {
        //        await userManager.AddClaimAsync(appUser, new Claim(type, claim));

        //        return Ok();
        //    }

        //    return BadRequest();
        //}

        [HttpPost("role")]
        public async Task<IActionResult> AddRole(string username, string rolename)
        {
            AppUser appUser = await userManager.FindByNameAsync(username);
            if (appUser != null)
            {
                IdentityRole role = await roleManager.FindByNameAsync(rolename);
           
                if (role == null)
                {
                    role = new IdentityRole(rolename);
                    await roleManager.CreateAsync(role);
                }

                await userManager.AddToRoleAsync(appUser, rolename);

                return Ok();
            }

            return BadRequest();
        }
    }
}