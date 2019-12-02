using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.Jint;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SapphireDb.Connection;
using SapphireDb.Connection.SSE;
using SapphireDb.Connection.Websocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SapphireDb.Command.Login;
using SapphireDb.Command.Renew;
using SapphireDb.Connection.Poll;
using SapphireDb.Nlb;

namespace SapphireDb.Extensions
{
    public static class SapphireDatabaseExtension
    {
        public static IApplicationBuilder UseSapphireDb(this IApplicationBuilder builder)
        {
            SapphireDatabaseOptions options = (SapphireDatabaseOptions)builder.ApplicationServices.GetService(typeof(SapphireDatabaseOptions));

            if (options.EnableBuiltinAuth)
            {
                builder.UseAuthentication();
            }

            builder.Map("/sapphire", (sapphireApp) =>
            {
                if (options.WebsocketInterface)
                {
                    sapphireApp.Map("/socket", (socket) =>
                    {
                        socket.UseWebSockets();
                        socket.UseMiddleware<WebsocketMiddleware>();
                    });
                }

                if (options.ServerSentEventsInterface || options.PollInterface)
                {
                    sapphireApp.Map("/api", (api) => { api.UseMiddleware<RestMiddleware>(); });
                }

                if (options.ServerSentEventsInterface)
                {
                    sapphireApp.Map("/sse", (sse) => { sse.UseMiddleware<SSEMiddleware>(); });
                }

                if (options.PollInterface)
                {
                    sapphireApp.Map("/poll", (poll) => { poll.UseMiddleware<PollMiddleware>(); });
                }

                if (options.Nlb.Enabled)
                {
                    sapphireApp.Map("/nlb", (nlb) => { nlb.UseMiddleware<NlbMiddleware>(); });
                }
            });

            return builder;
        }

        public static SapphireDatabaseBuilder AddSapphireDb(this IServiceCollection services, SapphireDatabaseOptions options = null)
        {
            if (options == null)
            {
                options = new SapphireDatabaseOptions();
            }

            services.AddSingleton(options);

            CommandExecutor commandExecutor = new CommandExecutor(options);
            services.AddSingleton(commandExecutor);

            foreach (KeyValuePair<string, Type> handler in commandExecutor.commandHandlerTypes)
            {
                services.AddTransient(handler.Value);
            }

            services.AddSingleton(new DbContextTypeContainer());

            services.AddScoped<SapphireDatabaseNotifier>();

            services.AddTransient<DbContextAccesor>();

            services.AddSingleton<ConnectionManager>();
            services.AddTransient<SapphireChangeNotifier>();

            services.AddSingleton<SapphireMessageSender>();

            ActionMapper actionMapper = new ActionMapper();
            services.AddSingleton(actionMapper);

            services.AddHttpClient<HttpClient>((client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
            services.AddTransient<NlbManager>();

            foreach (KeyValuePair<string, Type> handler in actionMapper.actionHandlerTypes)
            {
                services.AddTransient(handler.Value);
            }

            services.AddJsEngineSwitcher(jsOptions => jsOptions.DefaultEngineName = JintJsEngine.EngineName)
                .AddJint();

            return new SapphireDatabaseBuilder(services);
        }

        public static IServiceCollection AddSapphireAuth<TContextType, TUserType>(
            this IServiceCollection services,
            JwtOptions jwtOptions,
            Action<DbContextOptionsBuilder> dbContextOptionsAction = null,
            Action<IdentityOptions> identityOptionsAction = null) 
            where TContextType : SapphireAuthContext<TUserType>
            where TUserType : IdentityUser
        {
            SapphireDatabaseOptions options =
                (SapphireDatabaseOptions)services.FirstOrDefault(s => s.ServiceType == typeof(SapphireDatabaseOptions))?.ImplementationInstance;

            // ReSharper disable once PossibleNullReferenceException
            options.EnableBuiltinAuth = true;

            services.AddDbContext<TContextType>(dbContextOptionsAction, ServiceLifetime.Transient);
            services.AddTransient<AuthDbContextAccesor>();

            services.AddSingleton(new AuthDbContextTypeContainer() {
                DbContextType = typeof(TContextType),
                UserManagerType = typeof(UserManager<TUserType>),
                UserType = typeof(TUserType)
            });

            AddHandlers(services, options);
            AddIdentityProviders<TUserType, TContextType>(services, identityOptionsAction);
            AddAuthenticationProviders(services, jwtOptions);

            return services;
        }

        private static void AddHandlers(IServiceCollection services, SapphireDatabaseOptions options)
        {
            if (options.EnableAuthCommands)
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
            where TContextType : SapphireAuthContext<TUserType>
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
            services.AddTransient<IRoleStore<IdentityRole>, RoleStore<IdentityRole, SapphireAuthContext<TUserType>, string>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(RoleManager<IdentityRole>)));
            services.AddTransient<RoleManager<IdentityRole>>();

            services.Remove(services.FirstOrDefault(s => s.ServiceType == typeof(IUserStore<TUserType>)));
            services.AddTransient<IUserStore<TUserType>, UserStore<TUserType, IdentityRole, SapphireAuthContext<TUserType>, string>>();

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
