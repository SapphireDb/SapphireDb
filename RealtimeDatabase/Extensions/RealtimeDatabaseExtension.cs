using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Websocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Extensions
{
    public static class RealtimeDatabaseExtension
    {
        public static IApplicationBuilder UseRealtimeDatabase(this IApplicationBuilder builder)
        {
            builder.Map("/realtimedatabase", (realtimeApp) =>
            {
                realtimeApp.Map("/socket", (socket) =>
                {
                    socket.UseWebSockets();
                    socket.UseMiddleware<RealtimeDatabaseWebsocketMiddleware>();
                });

            });

            return builder;
        }

        public static IApplicationBuilder UseRealtimeAuth(this IApplicationBuilder builder)
        {
            builder.UseAuthentication();
            return builder;
        }

        public static IServiceCollection AddRealtimeDatabase<ContextType>(this IServiceCollection services)
            where ContextType : RealtimeDbContext
        {
            CommandHandlerMapper commandHandlerMapper = new CommandHandlerMapper();
            services.AddSingleton(commandHandlerMapper);

            foreach (KeyValuePair<string, Type> handler in commandHandlerMapper.commandHandlerTypes)
            {
                services.AddScoped(handler.Value);
            }

            services.AddSingleton(new DbContextTypeContainer() { DbContextType = typeof(ContextType) });
            services.AddScoped<DbContextAccesor>();

            services.AddScoped<RealtimeDatabaseNotifier>();

            services.AddSingleton<WebsocketConnectionManager>();
            services.AddScoped<WebsocketChangeNotifier>();
            services.AddScoped<WebsocketCommandHandler>();

            services.AddSingleton<RealtimeMessageSender>();

            ActionMapper actionMapper = new ActionMapper();
            services.AddSingleton(actionMapper);

            foreach (KeyValuePair<string, Type> handler in actionMapper.actionHandlerTypes)
            {
                services.AddTransient(handler.Value);
            }

            return services;
        }

        public static IServiceCollection AddRealtimeAuth<ContextType, UserType>(this IServiceCollection services, JwtOptions jwtOptions, Action<DbContextOptionsBuilder> dbContextOptionsAction = null, Action<IdentityOptions> identityOptionsAction = null) 
            where ContextType : RealtimeAuthContext<UserType>
            where UserType : IdentityUser
        {
            if (identityOptionsAction == null)
            {
                identityOptionsAction = options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 0;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                };
            }

            services.AddDbContext<ContextType>(dbContextOptionsAction);

            services.AddSingleton(jwtOptions);

            services.AddSingleton(new AuthDbContextTypeContainer() {
                DbContextType = typeof(ContextType),
                UserManagerType = typeof(UserManager<UserType>) });

            services.AddScoped<AuthDbContextAccesor>();

            services.AddScoped<JwtIssuer>();

            services.AddIdentity<UserType, IdentityRole>(identityOptionsAction).AddEntityFrameworkStores<ContextType>();

            services.AddAuthentication(cfg => {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg => {
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
                        string bearer = ctx.Request.Query["bearer"];
                        if (!String.IsNullOrEmpty(bearer))
                        {
                            ctx.Token = bearer;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(cfg =>
            {

            });

            return services;
        }
    }
}
