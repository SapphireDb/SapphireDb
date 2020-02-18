using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using SapphireDb.Helper;
using SapphireDb.Models;
using SapphireDb.Sync.Models;
using StackExchange.Redis;

namespace SapphireDb.Sync.Redis
{
    public class SapphireRedisSyncModule : ISapphireSyncModule
    {
        private readonly SapphireDatabaseOptions options;
        private readonly IConnectionMultiplexer redisMultiplexer;

        public SapphireRedisSyncModule(RedisStore redisStore, SapphireDatabaseOptions options)
        {
            this.options = options;
            redisMultiplexer = redisStore.RedisCache.Multiplexer;

            redisMultiplexer.GetSubscriber().Subscribe($"{options.Sync.Prefix}sapphiresync/*", (channel, message) =>
            {
                string channelPath = channel.ToString().Split('/').LastOrDefault();
                
                switch (channelPath)
                {
                    case "changes":
                        SendChangesRequest changesRequest = JsonConvert.DeserializeObject<SendChangesRequest>(message);
                        SyncRequestRequestReceived?.Invoke(changesRequest);
                        break;
                    case "publish":
                        SendPublishRequest publishRequest = JsonConvert.DeserializeObject<SendPublishRequest>(message);
                        SyncRequestRequestReceived?.Invoke(publishRequest);
                        break;
                    case "message":
                        SendMessageRequest messageRequest = JsonConvert.DeserializeObject<SendMessageRequest>(message);
                        SyncRequestRequestReceived?.Invoke(messageRequest);
                        break;
                }
            });
        }
        
        public void Publish(SyncRequest syncRequest)
        {
            string requestString = JsonHelper.Serialize(syncRequest);
            
            string path = syncRequest is SendPublishRequest ? "publish" :
                syncRequest is SendMessageRequest ? "message" : "changes";

            redisMultiplexer.GetSubscriber().Publish($"{options.Sync.Prefix}sapphiresync/{path}", requestString);
        }
        
        public event ISapphireSyncModule.SyncRequestReceivedHandler SyncRequestRequestReceived;
    }
}