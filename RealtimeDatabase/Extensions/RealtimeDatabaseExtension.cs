using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Websocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Extensions
{
    public static class RealtimeDatabaseExtension
    {
        public static IApplicationBuilder UseRealtimeDatabase(this IApplicationBuilder builder, bool useAuth = false)
        {
            if (useAuth)
            {
                builder.UseAuthentication();
            }

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

        public static IServiceCollection AddRealtimeDatabase<ContextType>(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextOptions = null, RealtimeDatabaseOptions options = null)
            where ContextType : RealtimeDbContext
        {
            services.AddDbContext<ContextType>(dbContextOptions, ServiceLifetime.Transient);

            if (options == null)
            {
                options = new RealtimeDatabaseOptions();
            }

            services.AddSingleton(options);

            CommandHandlerMapper commandHandlerMapper = new CommandHandlerMapper(options);
            services.AddSingleton(commandHandlerMapper);

            foreach (KeyValuePair<string, Type> handler in commandHandlerMapper.commandHandlerTypes)
            {
                services.AddTransient(handler.Value);
            }

            services.AddSingleton(new DbContextTypeContainer() { DbContextType = typeof(ContextType) });
            services.AddTransient<DbContextAccesor>();

            services.AddScoped<RealtimeDatabaseNotifier>();

            services.AddSingleton<WebsocketConnectionManager>();
            services.AddTransient<WebsocketChangeNotifier>();
            services.AddTransient<WebsocketCommandHandler>();

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
            services.AddDbContext<ContextType>(dbContextOptionsAction, ServiceLifetime.Transient);
            services.AddTransient<AuthDbContextAccesor>();

            services.AddSingleton(new AuthDbContextTypeContainer() {
                DbContextType = typeof(ContextType),
                UserManagerType = typeof(UserManager<UserType>),
                UserType = typeof(UserType)
            });

            AddHandlers(services);
            AddIdentityProviders<UserType, ContextType>(services, identityOptionsAction);
            AddAuthenticationProviders(services, jwtOptions);

            return services;
        }

        private static void AddHandlers(IServiceCollection services)
        {
            RealtimeDatabaseOptions realtimeDatabaseOptions =
                (RealtimeDatabaseOptions)services.FirstOrDefault(s => s.ServiceType == typeof(RealtimeDatabaseOptions))?.ImplementationInstance;

            if (realtimeDatabaseOptions.EnableAuthCommands)
            {
                CommandHandlerMapper commandHandlerMapper =
                    (CommandHandlerMapper)services.FirstOrDefault(s => s.ServiceType == typeof(CommandHandlerMapper))?.ImplementationInstance;

                foreach (KeyValuePair<string, Type> handler in commandHandlerMapper.authCommandHandlerTypes)
                {
                    services.AddTransient(handler.Value);
                }
            }
            else
            {
                services.AddTransient<LoginCommandHandler>();
                services.AddTransient<RenewCommandHandler>();
            }
        }

        private static void AddIdentityProviders<UserType, ContextType>(IServiceCollection services, Action<IdentityOptions> identityOptionsAction)
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

            services.AddIdentity<UserType, IdentityRole>(identityOptionsAction).AddEntityFrameworkStores<ContextType>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(IRoleStore<IdentityRole>)));
            services.AddTransient<IRoleStore<IdentityRole>, RoleStore<IdentityRole, RealtimeAuthContext<UserType>, string>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(RoleManager<IdentityRole>)));
            services.AddTransient<RoleManager<IdentityRole>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(IUserStore<UserType>)));
            services.AddTransient<IUserStore<UserType>, UserStore<UserType, IdentityRole, RealtimeAuthContext<UserType>, string>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(UserManager<UserType>)));
            services.AddTransient<UserManager<UserType>>();
        }

        private static void AddAuthenticationProviders(IServiceCollection services, JwtOptions jwtOptions)
        {
            services.AddSingleton(jwtOptions);
            services.AddTransient<JwtIssuer>();

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
        }
    }
}
