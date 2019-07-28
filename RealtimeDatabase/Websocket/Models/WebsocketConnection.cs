using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Primitives;

namespace RealtimeDatabase.Websocket.Models
{
    [DataContract]
    public class WebsocketConnection : IDisposable
    {
        public WebsocketConnection(WebSocket webSocket, HttpContext context)
        {
            Id = Guid.NewGuid();
            Subscriptions = new List<CollectionSubscription>();
            MessageSubscriptions = new Dictionary<string, string>();
            Websocket = webSocket;
            HttpContext = context;

            if (HttpContext.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent)) {
                UserAgent = userAgent.ToString();   
            }

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                UserId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            }

            Lock = new SemaphoreSlim(1, 1);
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string UserAgent { get; set; }

        public WebSocket Websocket { get; set; }

        public HttpContext HttpContext { get; set; }

        public List<CollectionSubscription> Subscriptions { get; set; }

        public Dictionary<string, string> MessageSubscriptions { get; set; }

        public string UsersSubscription { get; set; }

        public string RolesSubscription { get; set; }

        public SemaphoreSlim Lock { get; }

        public async Task Send(object message)
        {
            await Lock.WaitAsync();

            try
            {
                await Websocket.Send(message);
            }
            finally
            {
                Lock.Release();
            }
        }

        public void Dispose()
        {
            Websocket.Dispose();
            Subscriptions.ForEach(s => s.Dispose());
        }
    }
}
