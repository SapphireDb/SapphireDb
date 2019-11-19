using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Nlb.Models;

namespace RealtimeDatabase.Nlb
{
    public class NlbManager
    {
        private readonly RealtimeDatabaseOptions options;
        private readonly IHttpClientFactory httpClientFactory;

        public NlbManager(RealtimeDatabaseOptions options, IHttpClientFactory httpClientFactory)
        {
            this.options = options;
            this.httpClientFactory = httpClientFactory;
        }

        public void SendChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            if (options.Nlb.Enabled)
            {
                SendChangesRequest sendChangesRequest = new SendChangesRequest()
                {
                    Changes = changes,
                    DbType = dbContextType.FullName
                };

                string requestString = JsonHelper.Serialize(sendChangesRequest).Encrypt(options.Nlb.EncryptionKey);

                options.Nlb.Entries.ForEach(nlbEntry =>
                {
                    Task.Run(async () =>
                    {
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                            $"{(nlbEntry.Url.EndsWith('/') ? nlbEntry.Url : nlbEntry.Url + "/")}realtimedatabase/nlb");
                        request.Headers.Add("Secret", nlbEntry.Secret);
                        request.Content = new StringContent(requestString);

                        HttpClient client = httpClientFactory.CreateClient();
                        await client.SendAsync(request);
                    });
                });
            }
        }
    }
}
