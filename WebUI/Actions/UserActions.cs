using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WebUI.Data;
using WebUI.Data.Authentication;
using WebUI.Data.Models;
using WebUI.Data.ViewModels.Account;
using System.Collections.Generic;
using SapphireDb.Actions;

namespace WebUI.Actions
{
    public class UserActions : ActionHandlerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly JwtIssuer issuer;

        public UserActions(UserManager<AppUser> userManager, JwtIssuer issuer)
        {
            this.userManager = userManager;
            this.issuer = issuer;
        }

        public async Task<string> Login(string username, string password)
        {
            AppUser user = await userManager.FindByNameAsync(username);

            if (user != null)
            {
                if (await userManager.CheckPasswordAsync(user, password))
                {
                    return await issuer.GenerateEncodedToken(user);
                }
            }

            return null;
        }
        
        public List<AppUser> GetUsers()
        {
            return userManager.Users.ToList();
        }
    }
}
