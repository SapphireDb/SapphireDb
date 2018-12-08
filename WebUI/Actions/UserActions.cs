using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Data.Authentication;
using WebUI.Data.ViewModels.Account;

namespace WebUI.Actions
{
    public class UserActions : ActionHandlerBase
    {
        private UserManager<AppUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public UserActions(UserManager<AppUser> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            userManager = _userManager;
            roleManager = _roleManager;
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
