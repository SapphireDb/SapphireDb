using System;
using SapphireDb.Models;
using StackExchange.Redis;

namespace SapphireDb.Sync.Redis
{
    public class RedisStore
    {
        public RedisStore(SapphireDatabaseOptions options)
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options.Sync.ConnectionString));
        }

        private readonly Lazy<ConnectionMultiplexer> lazyConnection;
        
        private ConnectionMultiplexer Connection => lazyConnection.Value;

        public IDatabase RedisCache => Connection.GetDatabase();
    }
}