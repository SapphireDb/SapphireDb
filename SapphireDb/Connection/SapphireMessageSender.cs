using System;
using System.Collections.Generic;
using System.Linq;
using SapphireDb.Command.Message;
using SapphireDb.Command.SubscribeMessage;
using SapphireDb.Connection.Websocket;
using SapphireDb.Models;
using SapphireDb.Sync;

namespace SapphireDb.Connection
{
    public class SapphireMessageSender
    {
        public static readonly Dictionary<string, object> registeredMessageFilter = new Dictionary<string, object>();
        
        private readonly ConnectionManager connectionManager;
        private readonly SyncManager syncManager;

        public SapphireMessageSender(ConnectionManager connectionManager, SyncManager syncManager)
        {
            this.connectionManager = connectionManager;
            this.syncManager = syncManager;
        }

        public void Send(object message, string filter = null, object[] filterParameters = null, bool sync = true)
        {
            if (sync)
            {
                syncManager.SendMessage(message);
            }

            List<ConnectionBase> connections = connectionManager.connections.ToList();
            
            if (!string.IsNullOrEmpty(filter) && registeredMessageFilter.TryGetValue(filter, out object filterFunction))
            {
                if (filterFunction is Func<HttpInformation, bool> filterFunctionNoParameters)
                {
                    connections = connections
                        .Where((connection) => filterFunctionNoParameters(connection.Information))
                        .ToList();
                }
                else if (filterFunction is Func<HttpInformation, object[], bool> filterFunctionParameters)
                {
                    connections = connections
                        .Where((connection) => filterFunctionParameters(connection.Information, filterParameters))
                        .ToList();
                }
            }

            foreach (ConnectionBase connection in connections)
            {
                _ = connection.Send(new MessageResponse()
                {
                    Data = message
                });
            }
        }

        public void Publish(string topic, object message, bool sync = true)
        {
            if (sync)
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
