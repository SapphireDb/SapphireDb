using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Websocket.Models
{
    public class WebsocketConnection
    {
        public WebsocketConnection(WebSocket webSocket, HttpContext context)
        {
            Id = Guid.NewGuid();
            Subscriptions = new List<CollectionSubscription>();
            MessageSubscriptions = new Dictionary<string, string>();
            Websocket = webSocket;
            HttpContext = context;
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

        public async Task SendException<T>(CommandBase command, Exception exception)
            where T : ResponseBase
        {
            T response = Activator.CreateInstance<T>();

            response.ReferenceId = command.ReferenceId;
            response.Error = exception;

            await Send(response);
        }

        public async Task SendException<T>(CommandBase command, string message)
            where T : ResponseBase
        {
            await SendException<T>(command, new Exception(message));
        }
    }
}
