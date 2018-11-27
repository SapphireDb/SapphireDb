using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealtimeDatabase.Websocket
{
    public class RealtimeMessageSender
    {
        private WebsocketConnectionManager WebsocketConnectionManager;

        public RealtimeMessageSender(WebsocketConnectionManager websocketConnectionManager)
        {
            WebsocketConnectionManager = websocketConnectionManager;
        }

        public void Send(object message)
        {
            foreach (WebsocketConnection connection in WebsocketConnectionManager.connections)
            {
                lock (connection)
                {
                    connection.Websocket.Send(new MessageResponse() {
                        Data = message
                    });
                }
            }
        }

        public void Send(Func<WebsocketConnection, bool> filter, object message)
        {
            foreach (WebsocketConnection connection in WebsocketConnectionManager.connections.Where(filter))
            {
                lock (connection)
                {
                    connection.Websocket.Send(new MessageResponse()
                    {
                        Data = message
                    });
                }
            }
        }

        public void Publish(string topic, object message)
        {
            foreach (WebsocketConnection connection in 
                WebsocketConnectionManager.connections.Where(c => c.MessageSubscriptions.ContainsValue(topic)))
            {
                foreach (string subscriptionId in 
                    connection.MessageSubscriptions.Where(s => s.Value.ToLowerInvariant() == topic).Select(s => s.Key))
                {
                    lock (connection)
                    {
                        connection.Websocket.Send(new TopicResponse()
                        {
                            ReferenceId = subscriptionId,
                            Message = message
                        });
                    }
                }
            }
        }
    }
}
