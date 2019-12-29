using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Connection;
using SapphireDb.Connection.Poll;
using SapphireDb.Connection.SSE;
using SapphireDb.Connection.Websocket;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Nlb;

namespace SapphireDb.Extensions
{
    public static class SapphireDatabaseExtension
    {
        public static IApplicationBuilder UseSapphireDb(this IApplicationBuilder builder)
        {
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

            return new SapphireDatabaseBuilder(services);
        }
    }
}
