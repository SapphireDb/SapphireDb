using Microsoft.Extensions.Configuration;

namespace SapphireDb.RedisSync
{
    public class RedisSyncConfiguration
    {
        public RedisSyncConfiguration()
        {
            
        }
        
        public RedisSyncConfiguration(IConfigurationSection configurationSection)
        {
            Prefix = configurationSection[nameof(Prefix)];
            ConnectionString = configurationSection[nameof(ConnectionString)];
        }
        
        public string Prefix { get; set; }

        public string ConnectionString { get; set; }
    }
}