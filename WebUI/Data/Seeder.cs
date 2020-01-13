using System.Linq;
using Microsoft.AspNetCore.Identity;
using WebUI.Data.AuthDemo;
using WebUI.Data.Authentication;

namespace WebUI.Data
{
    public class Seeder
    {
        private readonly DemoContext demoContext;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AuthDemoContext authDemoContext;

        public Seeder(DemoContext demoContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
            AuthDemoContext authDemoContext)
        {
            this.demoContext = demoContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.authDemoContext = authDemoContext;
        }

        public void Execute()
        {
            demoContext.Database.EnsureCreated();

            if (!userManager.Users.Any())
            {
                roleManager.CreateAsync(new IdentityRole()
                {
                    Name = "admin"
                }).Wait();

                roleManager.CreateAsync(new IdentityRole()
                {
                    Name = "user"
                }).Wait();

                AppUser adminUser = new AppUser()
                {
                    Email = "admin@dev.de",
                    UserName = "admin",
                    FirstName = "Admin",
                    LastName = "User"
                };

                userManager.CreateAsync(adminUser, "admin").Wait();
                userManager.AddToRolesAsync(adminUser, new[] {"admin", "user"}).Wait();

                AppUser normalUser = new AppUser()
                {
                    Email = "user@dev.de",
                    UserName = "user",
                    FirstName = "Normal",
                    LastName = "User"
                };

                userManager.CreateAsync(normalUser, "user").Wait();
                userManager.AddToRolesAsync(normalUser, new[] {"user"}).Wait();
            }
            
            authDemoContext.RequiresAuthForQueryDemos.RemoveRange(authDemoContext.RequiresAuthForQueryDemos);
            authDemoContext.RequiresAuthForQueryDemos.AddRange(
                new RequiresAuthForQuery()
                {
                    Content = "Test 1"
                },
                new RequiresAuthForQuery()
                {
                    Content = "Test 2"
                }
            );
            
            authDemoContext.RequiresAdminForQueryDemos.RemoveRange(authDemoContext.RequiresAdminForQueryDemos);
            authDemoContext.RequiresAdminForQueryDemos.AddRange(
                new RequiresAdminForQuery()
                {
                    Content = "Test 1"
                },
                new RequiresAdminForQuery()
                {
                    Content = "Test 2"
                }
            );
            
            authDemoContext.CustomFunctionForQueryDemos.RemoveRange(authDemoContext.CustomFunctionForQueryDemos);
            authDemoContext.CustomFunctionForQueryDemos.AddRange(
                new CustomFunctionForQuery()
                {
                    Content = "Test 1"
                },
                new CustomFunctionForQuery()
                {
                    Content = "Test 2"
                }
            );

            authDemoContext.CustomFunctionPerEntryForQueryDemos.RemoveRange(authDemoContext.CustomFunctionPerEntryForQueryDemos);
            authDemoContext.CustomFunctionPerEntryForQueryDemos.AddRange(
                new CustomFunctionPerEntryForQuery()
                {
                    Content = "Test 1"
                },
                new CustomFunctionPerEntryForQuery()
                {
                    Content = "Test 2"
                }
            );
            
            authDemoContext.QueryFieldDemos.RemoveRange(authDemoContext.QueryFieldDemos);
            authDemoContext.QueryFieldDemos.AddRange(
                new QueryFields()
                {
                    Content = "Test 1",
                    Content2 = "Content 2.1",
                    Content3 = "Content 3.1"
                },
                new QueryFields()
                {
                    Content = "Test 2",
                    Content2 = "Content 2.2",
                    Content3 = "Content 3.2"
                }
            );
            
            authDemoContext.SaveChanges();
        }
    }
}