using RealtimeDatabase.Models.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;
using RealtimeDatabase.Connection.Websocket;

namespace RealtimeDatabase.Connection
{
    public class RealtimeMessageSender
    {
        private readonly RealtimeConnectionManager connectionManager;

        public RealtimeMessageSender(RealtimeConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        public void Send(object message)
        {
            foreach (ConnectionBase connection in connectionManager.connections)
            {
                _ = connection.Send(new MessageResponse()
                {
                    Data = message
                });
            }
        }

        public void Send(Func<ConnectionBase, bool> filter, object message)
        {
            foreach (ConnectionBase connection in connectionManager.connections.Where(filter))
            {
                _ = connection.Send(new MessageResponse()
                {
                    Data = message
                });
            }
        }

        public void Publish(string topic, object message)
        {
            foreach (ConnectionBase connection in 
                connectionManager.connections.Where(c => c.MessageSubscriptions.ContainsValue(topic)))
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
