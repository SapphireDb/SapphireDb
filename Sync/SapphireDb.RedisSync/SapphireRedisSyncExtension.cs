using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Extensions;
using SapphireDb.Sync;

namespace SapphireDb.RedisSync
{
    public static class SapphireRedisSyncExtension
    {
        public static SapphireDatabaseBuilder AddRedisSync(this SapphireDatabaseBuilder databaseBuilder, RedisSyncConfiguration redisSyncConfiguration)
        {
            databaseBuilder.serviceCollection.AddSingleton(redisSyncConfiguration);
            databaseBuilder.serviceCollection.AddSingleton<RedisStore>();
            databaseBuilder.serviceCollection.AddSingleton<ISapphireSyncModule, SapphireRedisSyncModule>();

            return databaseBuilder;
        }
    }
}