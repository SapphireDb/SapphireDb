using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Actions;
using System.Threading.Tasks;
using WebUI.Data.Authentication;
using WebUI.Data.ViewModels.Account;

namespace WebUI.Actions
{
    public class UserActions : ActionHandlerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserActions(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task CreateUser(NewAppUserViewModel model, string test)
        {
            AppUser userIdentity = new AppUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email
            };

            IdentityResult result = await userManager.CreateAsync(userIdentity, model.Password);
        }

        public async Task AddRole(string username, string rolename)
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
            }
        }
    }
}
