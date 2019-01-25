using System.Linq;
using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Models.Actions;
using System.Threading.Tasks;
using WebUI.Data;
using WebUI.Data.Authentication;
using WebUI.Data.Models;
using WebUI.Data.ViewModels.Account;

namespace WebUI.Actions
{
    public class UserActions : ActionHandlerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly RealtimeContext db;
        private readonly TestContext testdb;

        public UserActions(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, RealtimeContext db, TestContext testdb)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.db = db;
            this.testdb = testdb;
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

        public void Test()
        {
            testdb.Users.Add(new User()
            {
                FirstName = "TestBenutzer1",
                LastName = "TestBenutzer1",
                Username = "TestBenutzer1"
            });

            testdb.Users.Add(new User()
            {
                FirstName = "TestBenutzer2",
                LastName = "TestBenutzer2",
                Username = "TestBenutzer2"
            });

            testdb.Users.Add(new User()
            {
                FirstName = "TestBenutzer3",
                LastName = "TestBenutzer3",
                Username = "TestBenutzer3"
            });

            testdb.SaveChanges();

            testdb.Users.Remove(testdb.Users.FirstOrDefault());
            testdb.SaveChanges();

            testdb.Users.Add(new User()
            {
                FirstName = "TestBenutzer",
                LastName = "TestBenutzer",
                Username = "TestBenutzer"
            });
            testdb.SaveChanges();
        }
    }
}
