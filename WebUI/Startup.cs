using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileContextCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase.Extensions;
using RealtimeDatabase.Models.Actions;
using WebUI.Actions;
using WebUI.Data;
using WebUI.Data.Authentication;
using WebUI.Helper;

namespace WebUI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureJWTAuthService(Configuration.GetSection(nameof(JWTOptions)));

            //Register services
            services.AddRealtimeDatabase<RealtimeContext>(
                new ActionHandlerInformation("example", typeof(ExampleActions)));

            //services.AddDbContext<RealtimeContext>(cfg => cfg.UseFileContext(databasename: "realtime"));
            services.AddDbContext<RealtimeContext>(cfg => cfg.UseInMemoryDatabase("realtime"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(cfg => {
                cfg.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                cfg.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "dist";
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseAuthentication();

            //Add Middleware
            app.UseRealtimeDatabase();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvcWithDefaultRoute();

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
