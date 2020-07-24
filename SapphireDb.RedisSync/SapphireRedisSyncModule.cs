using Newtonsoft.Json;
using SapphireDb.Helper;
using SapphireDb.Sync;
using SapphireDb.Sync.Models;
using StackExchange.Redis;

namespace SapphireDb.RedisSync
{
    class SapphireRedisSyncModule : ISapphireSyncModule
    {
        private readonly RedisSyncConfiguration configuration;
        private readonly SyncContext syncContext;
        private readonly IConnectionMultiplexer redisMultiplexer;

        public SapphireRedisSyncModule(RedisStore redisStore, RedisSyncConfiguration configuration, SyncContext syncContext)
        {
            this.configuration = configuration;
            this.syncContext = syncContext;
            redisMultiplexer = redisStore.RedisCache.Multiplexer;

            redisMultiplexer.GetSubscriber().Subscribe($"{configuration.Prefix}sapphiresync/*/*", (channel, message) =>
            {
                string[] channelParts = channel.ToString().Split('/');
                string senderSessionId = channelParts[^2];

                if (syncContext.SessionId.ToString() == senderSessionId)
                {
                    return;
                }
                
                string channelPath = channelParts[^1];
                
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

            redisMultiplexer.GetSubscriber().Publish($"{configuration.Prefix}sapphiresync/{syncContext.SessionId}/{path}", requestString);
        }
        
        public event ISapphireSyncModule.SyncRequestReceivedHandler SyncRequestRequestReceived;
    }
}