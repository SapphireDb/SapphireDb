using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SapphireDb.Helper;
using SapphireDb.Sync;
using SapphireDb.Sync.Http;
using SapphireDb.Sync.Models;

namespace SapphireDb.HttpSync
{
    class SapphireHttpSyncModule : ISapphireSyncModule
    {
        private readonly HttpSyncConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public SapphireHttpSyncModule(HttpSyncConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }
        
        public void Publish(SyncRequest syncRequest)
        {
            string requestString = JsonHelper.Serialize(syncRequest);

            string path = syncRequest is SendPublishRequest ? "publish" :
                syncRequest is SendMessageRequest ? "message" : "changes";
            
            configuration.Entries.Where(entry => !string.IsNullOrEmpty(entry.Url)).ToList().ForEach(nlbEntry =>
            {
                Task.Run(async () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                        $"{(nlbEntry.Url.EndsWith('/') ? nlbEntry.Url : nlbEntry.Url + "/")}sapphiresync/{path}");
                    request.Headers.Add("Secret", nlbEntry.Secret);
                    request.Headers.Add("OriginId", configuration.Id);
                    request.Content = new StringContent(requestString);

                    HttpClient client = httpClientFactory.CreateClient();
                    await client.SendAsync(request);
                });
            });
        }

        public void Received(SyncRequest syncRequest)
        {
            SyncRequestRequestReceived?.Invoke(syncRequest);
        }
        
        public event ISapphireSyncModule.SyncRequestReceivedHandler SyncRequestRequestReceived;
    }
}