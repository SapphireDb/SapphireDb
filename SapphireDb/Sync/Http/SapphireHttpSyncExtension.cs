using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Extensions;

namespace SapphireDb.Sync.Http
{
    public static class SapphireHttpSyncExtension
    {
        public static SapphireDatabaseBuilder AddHttpSync(this SapphireDatabaseBuilder databaseBuilder)
        {
            databaseBuilder.serviceCollection.AddHttpClient<HttpClient>((client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
            
            databaseBuilder.serviceCollection.AddSingleton<ISapphireSyncModule, SapphireHttpSyncModule>();

            return databaseBuilder;
        }

        public static IApplicationBuilder UseSapphireHttpSync(this IApplicationBuilder builder)
        {
            builder.Map("/sapphiresync", (nlb) => { nlb.UseMiddleware<SapphireHttpSyncMiddleware>(); });

            return builder;
        }
    }
}