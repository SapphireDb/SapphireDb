using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SapphireDb.Command.Subscribe;
using SapphireDb.Helper;
using SapphireDb.Models;
using SapphireDb.Sync.Models;

namespace SapphireDb.Sync
{
    public class SyncManager
    {
        private readonly SapphireDatabaseOptions options;
        private readonly IHttpClientFactory httpClientFactory;

        public SyncManager(SapphireDatabaseOptions options, IHttpClientFactory httpClientFactory)
        {
            this.options = options;
            this.httpClientFactory = httpClientFactory;
        }

        public void SendChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            SendChangesRequest sendChangesRequest = new SendChangesRequest()
            {
                Changes = changes,
                DbType = dbContextType.FullName
            };

            SendToNlbs(sendChangesRequest, "changes");
        }

        public void SendPublish(string topic, object message, bool retain)
        {
            SendPublishRequest sendPublishRequest = new SendPublishRequest()
            {
                Topic = topic,
                Message = message,
                Retain = retain
            };

            SendToNlbs(sendPublishRequest, "publish");
        }

        public void SendMessage(object message, string filter, object[] filterParameters)
        {
            SendMessageRequest sendMessageRequest = new SendMessageRequest()
            {
                Message = message,
                Filter = filter,
                FilterParameters = filterParameters
            };

            SendToNlbs(sendMessageRequest, "message");
        }

        private void SendToNlbs(object messageObject, string path)
        {
            if (!options.Sync.Enabled)
            {
                return;
            }

            string requestString = JsonHelper.Serialize(messageObject).Encrypt(options.Sync.EncryptionKey);

            options.Sync.Entries.Where(entry => !string.IsNullOrEmpty(entry.Url)).ToList().ForEach(nlbEntry =>
            {
                Task.Run(async () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                        $"{(nlbEntry.Url.EndsWith('/') ? nlbEntry.Url : nlbEntry.Url + "/")}sapphire/sync/{path}");
                    request.Headers.Add("Secret", nlbEntry.Secret);
                    request.Headers.Add("OriginId", options.Sync.Id);
                    request.Content = new StringContent(requestString);

                    HttpClient client = httpClientFactory.CreateClient();
                    await client.SendAsync(request);
                });
            });
        }
    }
}
