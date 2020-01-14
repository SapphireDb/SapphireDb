using System;
using System.Linq;
using SapphireDb.Command.Message;
using SapphireDb.Command.SubscribeMessage;
using SapphireDb.Connection.Websocket;
using SapphireDb.Sync;

namespace SapphireDb.Connection
{
    public class SapphireMessageSender
    {
        private readonly ConnectionManager connectionManager;
        private readonly SyncManager syncManager;

        public SapphireMessageSender(ConnectionManager connectionManager, SyncManager syncManager)
        {
            this.connectionManager = connectionManager;
            this.syncManager = syncManager;
        }

        public void Send(object message, bool noNlb = false)
        {
            if (!noNlb)
            {
                syncManager.SendMessage(message);
            }

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

        public void Publish(string topic, object message, bool noNlb = false)
        {
            if (!noNlb)
            {
                syncManager.SendPublish(topic, message);
            }

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
