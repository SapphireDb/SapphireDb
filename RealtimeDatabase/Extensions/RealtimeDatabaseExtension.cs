using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Websocket;
using System;
using System.Collections.Generic;
using System.Text;

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

        public static IServiceCollection AddRealtimeDatabase<ContextType>(this IServiceCollection services) where ContextType : RealtimeDbContext
        {
            services.AddSingleton<CommandHandlerMapper>();
            services.AddSingleton(new DbContextTypeContainer() { DbContextType = typeof(ContextType) });
            services.AddScoped<DbContextAccesor>();

            services.AddScoped<RealtimeDatabaseNotifier>();

            services.AddSingleton<WebsocketConnectionManager>();
            services.AddScoped<WebsocketChangeNotifier>();
            services.AddScoped<WebsocketCommandHandler>();
            return services;
        }
    }
}
