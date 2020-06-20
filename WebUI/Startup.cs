using System.Threading.Tasks;
using FileContextCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SapphireDb.Extensions;
using SapphireDb.Models;
using WebUI.Actions;
using WebUI.Data;
using WebUI.Data.Authentication;
using WebUI.Data.DemoDb;

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
                .AddContext<RealtimeContext>(cfg => cfg.UseInMemoryDatabase(databaseName: "realtime"))
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
                .AddContext<AuthDemoContext>(cfg => cfg.UseInMemoryDatabase("authDemo"), "authDemo")
                .AddMessageFilter("role", (i, parameters) => i.User.IsInRole((string) parameters[0]))
                .AddTopicConfiguration("admin", i => i.User.IsInRole("admin"), i => i.User.IsInRole("admin"));
                // .AddRedisSync();
                // .AddHttpSync();

            // services.AddMvc();
            
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

            services.AddTransient<Seeder>();
            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Seeder seeder)
        {
            seeder.Execute();
            
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
            // app.UseSapphireHttpSync();
            
            app.Run(async context =>
            {
                context.Response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                await context.Response.WriteAsync(
                    "SapphireDb Documentation Server. Visit <a href=\"https://sapphire-db.com\">https://sapphire-db.com</a> for more details.");
            });
                
            //app.UseMvcWithDefaultRoute();
        }
    }
}