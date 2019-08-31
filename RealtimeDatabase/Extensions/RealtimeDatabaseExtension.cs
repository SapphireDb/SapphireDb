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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Connection.SSE;
using RealtimeDatabase.Connection.Websocket;

namespace RealtimeDatabase.Extensions
{
    public static class RealtimeDatabaseExtension
    {
        public static IApplicationBuilder UseRealtimeDatabase(this IApplicationBuilder builder)
        {
            RealtimeDatabaseOptions options = (RealtimeDatabaseOptions)builder.ApplicationServices.GetService(typeof(RealtimeDatabaseOptions));

            if (options.EnableBuiltinAuth)
            {
                builder.UseAuthentication();
            }

            builder.Map("/realtimedatabase", (realtimeApp) =>
            {
                realtimeApp.Map("/socket", (socket) =>
                {
                    socket.UseWebSockets();
                    socket.UseMiddleware<WebsocketMiddleware>();
                });

                realtimeApp.Map("/sse", (sse) => { sse.UseMiddleware<SSEMiddleware>(); });

                if (options.RestFallback)
                {
                    realtimeApp.Map("/api", (api) => { api.UseMiddleware<RestMiddleware>(); });
                }
            });

            return builder;
        }

        public static RealtimeDatabaseBuilder AddRealtimeDatabase(this IServiceCollection services, RealtimeDatabaseOptions options = null)
        {
            if (options == null)
            {
                options = new RealtimeDatabaseOptions();
            }

            services.AddSingleton(options);

            CommandExecutor commandExecutor = new CommandExecutor(options);
            services.AddSingleton(commandExecutor);

            foreach (KeyValuePair<string, Type> handler in commandExecutor.commandHandlerTypes)
            {
                services.AddTransient(handler.Value);
            }

            services.AddSingleton(new DbContextTypeContainer());

            services.AddScoped<RealtimeDatabaseNotifier>();

            services.AddTransient<DbContextAccesor>();

            services.AddSingleton<RealtimeConnectionManager>();
            services.AddTransient<RealtimeChangeNotifier>();

            services.AddSingleton<RealtimeMessageSender>();

            ActionMapper actionMapper = new ActionMapper();
            services.AddSingleton(actionMapper);

            foreach (KeyValuePair<string, Type> handler in actionMapper.actionHandlerTypes)
            {
                services.AddTransient(handler.Value);
            }

            services.AddJsEngineSwitcher(jsOptions => jsOptions.DefaultEngineName = ChakraCoreJsEngine.EngineName)
                .AddChakraCore();

            return new RealtimeDatabaseBuilder(services);
        }

        public static IServiceCollection AddRealtimeAuth<TContextType, TUserType>(
            this IServiceCollection services,
            JwtOptions jwtOptions,
            Action<DbContextOptionsBuilder> dbContextOptionsAction = null,
            Action<IdentityOptions> identityOptionsAction = null) 
            where TContextType : RealtimeAuthContext<TUserType>
            where TUserType : IdentityUser
        {
            RealtimeDatabaseOptions realtimeDatabaseOptions =
                (RealtimeDatabaseOptions)services.FirstOrDefault(s => s.ServiceType == typeof(RealtimeDatabaseOptions))?.ImplementationInstance;

            // ReSharper disable once PossibleNullReferenceException
            realtimeDatabaseOptions.EnableBuiltinAuth = true;

            services.AddDbContext<TContextType>(dbContextOptionsAction, ServiceLifetime.Transient);
            services.AddTransient<AuthDbContextAccesor>();

            services.AddSingleton(new AuthDbContextTypeContainer() {
                DbContextType = typeof(TContextType),
                UserManagerType = typeof(UserManager<TUserType>),
                UserType = typeof(TUserType)
            });

            AddHandlers(services, realtimeDatabaseOptions);
            AddIdentityProviders<TUserType, TContextType>(services, identityOptionsAction);
            AddAuthenticationProviders(services, jwtOptions);

            return services;
        }

        private static void AddHandlers(IServiceCollection services, RealtimeDatabaseOptions realtimeDatabaseOptions)
        {
            if (realtimeDatabaseOptions.EnableAuthCommands)
            {
                CommandExecutor commandHandlerMapper =
                    (CommandExecutor)services.FirstOrDefault(s => s.ServiceType == typeof(CommandExecutor))?.ImplementationInstance;

                if (commandHandlerMapper != null)
                {
                    foreach (KeyValuePair<string, Type> handler in commandHandlerMapper.authCommandHandlerTypes)
                    {
                        services.AddTransient(handler.Value);
                    }
                }
            }
            else
            {
                services.AddTransient<LoginCommandHandler>();
                services.AddTransient<RenewCommandHandler>();
            }
        }

        private static void AddIdentityProviders<TUserType, TContextType>(IServiceCollection services, Action<IdentityOptions> identityOptionsAction)
            where TContextType : RealtimeAuthContext<TUserType>
            where TUserType : IdentityUser
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

            services.AddIdentity<TUserType, IdentityRole>(identityOptionsAction).AddEntityFrameworkStores<TContextType>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(IRoleStore<IdentityRole>)));
            services.AddTransient<IRoleStore<IdentityRole>, RoleStore<IdentityRole, RealtimeAuthContext<TUserType>, string>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(RoleManager<IdentityRole>)));
            services.AddTransient<RoleManager<IdentityRole>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(IUserStore<TUserType>)));
            services.AddTransient<IUserStore<TUserType>, UserStore<TUserType, IdentityRole, RealtimeAuthContext<TUserType>, string>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(UserManager<TUserType>)));
            services.AddTransient<UserManager<TUserType>>();
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
                        if (!string.IsNullOrEmpty(bearer))
                        {
                            ctx.Token = bearer;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();
        }
    }
}
