using System;
using System.Linq;
using FileContextCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using SapphireDb.Models.Auth;
using WebUI.Actions;
using WebUI.Data;
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

            services.AddDbContext<TestContext>(cfg => cfg.UseInMemoryDatabase("test"));

            SapphireDatabaseOptions options = new SapphireDatabaseOptions(Configuration.GetSection("Sapphire"));

            bool usePostgres = Configuration.GetValue<bool>("UsePostgres");
            
            //Register services
            SapphireDatabaseBuilder realtimeBuilder = services.AddSapphireDb(options)
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
                }, "demo");

            services.AddSapphireAuth<SapphireAuthContext<AppUser>, AppUser>(new JwtOptions(Configuration.GetSection(nameof(JwtOptions))),
                cfg => cfg.UseFileContextDatabase(databaseName: "auth"));


            services.AddMvc();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "dist";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RealtimeContext db, DemoContext demoContext)
        {
            demoContext.Database.EnsureCreated();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

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
