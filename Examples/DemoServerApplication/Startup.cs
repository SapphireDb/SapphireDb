using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoServerApplication.Data;
using FileContextCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase.Extensions;
using RealtimeDatabase.Models;

namespace DemoServerApplication
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
            //Register services
            services.AddRealtimeDatabase(new RealtimeDatabaseOptions()
                {
                    // Options
                })
                .AddContext<RealtimeContext>(cfg => cfg.UseFileContext(databasename: "realtime"));

            //services.AddRealtimeAuth<RealtimeAuthContext<AppUser>, AppUser>(new JwtOptions(Configuration.GetSection(nameof(JwtOptions))), cfg => cfg.UseFileContext(databasename: "auth"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            //app.UseRealtimeAuth();
            app.UseRealtimeDatabase();

            app.Run(request =>
            {
                return request.Response.WriteAsync("RealtimeDatabase Demo Server");
            });
        }
    }
}
