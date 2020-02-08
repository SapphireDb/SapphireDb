using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Extensions;

namespace SapphireDb.Sync.Redis
{
    public static class SapphireRedisSyncExtension
    {
        public static SapphireDatabaseBuilder AddRedisSync(this SapphireDatabaseBuilder databaseBuilder)
        {
            databaseBuilder.serviceCollection.AddSingleton<RedisStore>();
            databaseBuilder.serviceCollection.AddSingleton<ISapphireSyncModule, SapphireRedisSyncModule>();

            return databaseBuilder;
        }
    }
}