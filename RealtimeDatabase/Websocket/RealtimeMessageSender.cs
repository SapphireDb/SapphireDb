using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    public class RealtimeMessageSender
    {
        private readonly WebsocketConnectionManager websocketConnectionManager;

        public RealtimeMessageSender(WebsocketConnectionManager websocketConnectionManager)
        {
            this.websocketConnectionManager = websocketConnectionManager;
        }

        public void Send(object message)
        {
            foreach (WebsocketConnection connection in websocketConnectionManager.connections)
            {
                _ = connection.Send(new MessageResponse()
                {
                    Data = message
                });
            }
        }

        public void Send(Func<WebsocketConnection, bool> filter, object message)
        {
            foreach (WebsocketConnection connection in websocketConnectionManager.connections.Where(filter))
            {
                _ = connection.Send(new MessageResponse()
                {
                    Data = message
                });
            }
        }

        public void Publish(string topic, object message)
        {
            foreach (WebsocketConnection connection in 
                websocketConnectionManager.connections.Where(c => c.MessageSubscriptions.ContainsValue(topic)))
            {
                foreach (string subscriptionId in 
                    connection.MessageSubscriptions.Where(s => s.Value.ToLowerInvariant() == topic).Select(s => s.Key))
                {
                    _ = connection.Send(new TopicResponse()
                    {
                        ReferenceId = subscriptionId,
                        Message = message
                    });
                }
            }
        }
    }
}
