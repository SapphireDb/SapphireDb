using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Connection;
using SapphireDb.Connection.Poll;
using SapphireDb.Connection.SSE;
using SapphireDb.Connection.Websocket;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.SapphireApiBuilder;
using SapphireDb.Sync;
using SapphireDb.Sync.Models;

namespace SapphireDb.Extensions
{
    public static class SapphireDatabaseExtension
    {
        public static IApplicationBuilder UseSapphireDb(this IApplicationBuilder builder)
        {
            // Invoke instance of sync manager on startup
            builder.ApplicationServices.GetService(typeof(SyncManager));
            
            SapphireDatabaseOptions options = (SapphireDatabaseOptions)builder.ApplicationServices.GetService(typeof(SapphireDatabaseOptions));

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

                if (options.ServerSentEventsInterface || options.PollInterface || options.RestInterface)
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

                sapphireApp.Map("/authToken", (authToken) =>
                {
                    authToken.Run(async (context) =>
                    {
                        bool authenticated = context.Request.Method == "POST" && context.User.Identity.IsAuthenticated;
                        await context.Response.WriteAsync(authenticated.ToString().ToLowerInvariant());
                    });
                });
            });

            ExecuteApiBuilders(builder.ApplicationServices);
            
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
            
            services.AddSingleton<SubscriptionManager>();
            services.AddSingleton<MessageSubscriptionManager>();

            services.AddScoped<SapphireDatabaseNotifier>();
            services.AddScoped<ISapphireDatabaseNotifier, SapphireDatabaseNotifier>();

            services.AddTransient<DbContextAccesor>();

            services.AddSingleton<ConnectionManager>();
            services.AddTransient<SapphireChangeNotifier>();

            services.AddSingleton<SapphireMessageSender>();

            services.AddSingleton<SapphireStreamHelper>();
            
            ActionMapper actionMapper = new ActionMapper();
            services.AddSingleton(actionMapper);
            
            services.AddSingleton<SyncContext>();
            services.AddSingleton<SyncManager>();

            foreach (KeyValuePair<string, Type> handler in actionMapper.actionHandlerTypes)
            {
                services.AddTransient(handler.Value);
            }
            
            IEnumerable<Type> modelConfigurationTypes = Assembly.GetCallingAssembly().GetTypes()
                .Where(t => typeof(ISapphireModelConfiguration).IsAssignableFrom(t));

            foreach (Type modelConfigurationType in modelConfigurationTypes)
            {
                services.AddTransient(typeof(ISapphireModelConfiguration), modelConfigurationType);
            }
            
            IEnumerable<Type> actionHandlerConfigurationTypes = Assembly.GetCallingAssembly().GetTypes()
                .Where(t => typeof(ISapphireActionConfiguration).IsAssignableFrom(t));

            foreach (Type actionHandlerConfigurationType in actionHandlerConfigurationTypes)
            {
                services.AddTransient(typeof(ISapphireActionConfiguration), actionHandlerConfigurationType);
            }

            return new SapphireDatabaseBuilder(services);
        }

        private static void ExecuteApiBuilders(IServiceProvider serviceProvider)
        {
            IEnumerable<ISapphireModelConfiguration> sapphireModelConfigurations = serviceProvider.GetServices<ISapphireModelConfiguration>();

            foreach (ISapphireModelConfiguration sapphireModelConfiguration in sapphireModelConfigurations)
            {
                Type modelConfigurationType = sapphireModelConfiguration.GetType();

                MethodInfo configureMethod = modelConfigurationType.GetMethod("Configure",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                Type modelBuilderType = configureMethod?.GetParameters()[0].ParameterType;
                object modelBuilder = Activator.CreateInstance(modelBuilderType);

                configureMethod?.Invoke(sapphireModelConfiguration, new object[] { modelBuilder });
            }
            
            IEnumerable<ISapphireActionConfiguration> sapphireActionConfigurations = serviceProvider.GetServices<ISapphireActionConfiguration>();

            foreach (ISapphireActionConfiguration sapphireActionConfiguration in sapphireActionConfigurations)
            {
                Type actionConfigurationType = sapphireActionConfiguration.GetType();

                MethodInfo configureMethod = actionConfigurationType.GetMethod("Configure",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                Type actionHandlerBuilderType = configureMethod?.GetParameters()[0].ParameterType;
                object actionHandlerBuilder = Activator.CreateInstance(actionHandlerBuilderType);

                configureMethod?.Invoke(sapphireActionConfiguration, new object[] { actionHandlerBuilder });
            }
        }
    }
}
