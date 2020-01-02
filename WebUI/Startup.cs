using System;
using System.Linq;
using System.Threading.Tasks;
using FileContextCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Npgsql;
using SapphireDb;
using SapphireDb.Command;
using SapphireDb.Command.Execute;
using SapphireDb.Extensions;
using SapphireDb.Models;
using WebUI.Actions;
using WebUI.Data;
using WebUI.Data.AuthDemo;
using WebUI.Data.Authentication;
using WebUI.Data.Models;

namespace WebUI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            SapphireDatabaseOptions options = new SapphireDatabaseOptions(Configuration.GetSection("Sapphire"));

            bool usePostgres = Configuration.GetValue<bool>("UsePostgres");

            //Register services
            services.AddSapphireDb(options)
                .AddContext<RealtimeContext>(cfg => cfg.UseFileContextDatabase(databaseName: "realtime"))
                .AddContext<DemoContext>(cfg =>
                {
                    if (usePostgres)
                    {
                        cfg.UseNpgsql("User ID=realtime;Password=pw1234;Host=localhost;Port=5432;Database=realtime;");
                    }
                    else
                    {
                        cfg.UseInMemoryDatabase("demoCtx");
                    }
                }, "demo")
                .AddContext<AuthDemoContext>(cfg => cfg.UseInMemoryDatabase("authDemo"), "authDemo");

            services.AddMvc();

            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "dist"; });

            /* Auth Demo */
            services.AddDbContext<IdentityDbContext<AppUser>>(cfg => cfg.UseFileContextDatabase(databaseName: "auth"));

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 2;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<IdentityDbContext<AppUser>>();

            JwtOptions jwtOptions = new JwtOptions(Configuration.GetSection(nameof(JwtOptions)));
            services.AddSingleton(jwtOptions);
            services.AddTransient<JwtIssuer>();

            services.AddAuthentication(cfg =>
            {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = jwtOptions.TokenValidationParameters;
                cfg.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        return Task.CompletedTask;
                    },
                    OnMessageReceived = ctx =>
                    {
                        string authorizationToken = ctx.Request.Query["authorization"];
                        if (!string.IsNullOrEmpty(authorizationToken))
                        {
                            ctx.Token = authorizationToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("requireAdmin", b => b.RequireRole("admin"));
                config.AddPolicy("requireUser", b => b.RequireRole("user"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DemoContext demoContext,
            UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AuthDemoContext authDemoContext)
        {
            // Generation for Demo
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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCors(cfg => cfg.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            
            /* Auth Demo */
            app.UseAuthentication();
            // app.UseAuthorization();

            //Add Middleware
            app.UseSapphireDb();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            //app.UseMvcWithDefaultRoute();

            app.UseSpa(spa =>
            {
                if (env.IsDevelopment() || env.EnvironmentName == "pg")
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}