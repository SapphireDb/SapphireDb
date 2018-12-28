using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

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
        }

        public Guid Id { get; set; }

        public WebSocket Websocket { get; set; }

        public HttpContext HttpContext { get; set; }

        public List<CollectionSubscription> Subscriptions { get; set; }

        public Dictionary<string, string> MessageSubscriptions { get; set; }

        public string UsersSubscription { get; set; }

        public string RolesSubscription { get; set; }
    }
}
