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
using RealtimeDatabase;
using RealtimeDatabase.Extensions;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Models.Commands;
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

            RealtimeDatabaseOptions options = new RealtimeDatabaseOptions(Configuration.GetSection("RealtimeDatabase"));
            Func<CommandBase, HttpContext, bool> oldFunc = options.CanExecuteCommand;
            options.CanExecuteCommand = (command, context) => true || command is ExecuteCommand || oldFunc(command, context);

            //Register services
            RealtimeDatabaseBuilder realtimeBuilder = services.AddRealtimeDatabase(options)
                .AddContext<RealtimeContext>(
                    cfg => cfg.UseFileContextDatabase(databaseName: "realtime") /*cfg.UseInMemoryDatabase("realtime")*/)
                .AddContext<DemoContext>(cfg => cfg.UseInMemoryDatabase("demoCtx"), "demo");

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

            services.AddRealtimeAuth<RealtimeAuthContext<AppUser>, AppUser>(new JwtOptions(Configuration.GetSection(nameof(JwtOptions))),
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RealtimeContext db)
        {
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
            app.UseRealtimeDatabase();

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
