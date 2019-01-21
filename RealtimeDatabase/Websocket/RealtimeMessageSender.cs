using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    public class RealtimeMessageSender
    {
        private WebsocketConnectionManager WebsocketConnectionManager;

        public RealtimeMessageSender(WebsocketConnectionManager websocketConnectionManager)
        {
            WebsocketConnectionManager = websocketConnectionManager;
        }

        public async Task Send(object message)
        {
            foreach (WebsocketConnection connection in WebsocketConnectionManager.connections)
            {
                await connection.Send(new MessageResponse()
                {
                    Data = message
                });
            }
        }

        public async Task Send(Func<WebsocketConnection, bool> filter, object message)
        {
            foreach (WebsocketConnection connection in WebsocketConnectionManager.connections.Where(filter))
            {
                await connection.Send(new MessageResponse()
                {
                    Data = message
                });
            }
        }

        public async Task Publish(string topic, object message)
        {
            foreach (WebsocketConnection connection in 
                WebsocketConnectionManager.connections.Where(c => c.MessageSubscriptions.ContainsValue(topic)))
            {
                foreach (string subscriptionId in 
                    connection.MessageSubscriptions.Where(s => s.Value.ToLowerInvariant() == topic).Select(s => s.Key))
                {
                    await connection.Send(new TopicResponse()
                    {
                        ReferenceId = subscriptionId,
                        Message = message
                    });
                }
            }
        }
    }
}
