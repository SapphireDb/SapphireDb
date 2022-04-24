using System;
using StackExchange.Redis;

namespace SapphireDb.RedisSync
{
    class RedisStore
    {
        public RedisStore(RedisSyncConfiguration configuration)
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configuration.ConnectionString));
        }

        private readonly Lazy<ConnectionMultiplexer> lazyConnection;
        
        private ConnectionMultiplexer Connection => lazyConnection.Value;

        public IDatabase RedisCache => Connection.GetDatabase();
    }
}