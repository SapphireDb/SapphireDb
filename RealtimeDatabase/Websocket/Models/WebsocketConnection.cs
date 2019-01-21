using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket.Models
{
    public class WebsocketConnection
    {
        public WebsocketConnection(WebSocket _webSocket, HttpContext _context)
        {
            Id = Guid.NewGuid();
            Subscriptions = new List<CollectionSubscription>();
            MessageSubscriptions = new Dictionary<string, string>();
            Websocket = _webSocket;
            HttpContext = _context;
            Lock = new SemaphoreSlim(1, 1);
        }

        public Guid Id { get; set; }

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
    }
}
