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
            Func<CommandBase, HttpInformation, bool> oldFunc = options.CanExecuteCommand;
            options.CanExecuteCommand = (command, context) => true || command is ExecuteCommand || oldFunc(command, context);
            options.IsAllowedForTopicPublish = (context, topic) => true;
            options.IsAllowedForTopicSubscribe = (context, topic) => true;
            options.IsAllowedToSendMessages = (context) => true;

            //Register services
            SapphireDatabaseBuilder realtimeBuilder = services.AddSapphireDb(options)
                .AddContext<RealtimeContext>(
                    cfg => cfg.UseFileContextDatabase(databaseName: "realtime") /*cfg.UseInMemoryDatabase("realtime")*/)
                //.AddContext<DemoContext>(cfg => cfg.UseFileContextDatabase(), "demo");
                //.AddContext<DemoContext>(cfg => cfg.UseInMemoryDatabase("demoCtx"), "demo");
                .AddContext<DemoContext>(cfg => cfg.UseNpgsql("User ID=realtime;Password=pw1234;Host=localhost;Port=5432;Database=realtime;"), "demo");

            RealtimeContext db = services.BuildServiceProvider().GetService<RealtimeContext>();

            if (db != null)
            {
                Config dbName = db.Configs.FirstOrDefault(cfg => cfg.Key == "DbName");
                DbActions.DbName = dbName?.Value;
            }

            realtimeBuilder.AddContext<SecondRealtimeContext>(cfg =>
                {
                    //if (string.IsNullOrEmpty(DbActions.DbName))
                    //{
                    //    throw new Exception("DbName not configured");
                    //}

                    cfg.UseFileContextDatabase(databaseName: DbActions.DbName ?? "test"); /*cfg.UseInMemoryDatabase("second")*/
                }, "second");

            services.AddSapphireAuth<SapphireAuthContext<AppUser>, AppUser>(new JwtOptions(Configuration.GetSection(nameof(JwtOptions))),
                cfg => /*cfg.UseInMemoryDatabase()*/cfg.UseFileContextDatabase(databaseName: "auth"));


            services.AddMvc().AddJsonOptions(cfg =>
            {
                //cfg.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                //cfg.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            });

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
                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
