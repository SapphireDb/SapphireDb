using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Command.Message;
using SapphireDb.Command.SubscribeMessage;
using SapphireDb.Connection.Websocket;
using SapphireDb.Models;
using SapphireDb.Sync;

namespace SapphireDb.Connection
{
    public class SapphireMessageSender
    {
        public static readonly ConcurrentDictionary<string, object> RetainedTopicMessages = new ConcurrentDictionary<string, object>();
        public static readonly Dictionary<string, object> RegisteredMessageFilter = new Dictionary<string, object>();
        
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
                syncManager.SendMessage(message, filter, filterParameters);
            }
            
            Func<HttpInformation, bool> filterFunctionNoParameters = null;
            Func<HttpInformation, object[], bool> filterFunctionParameters = null;
            
            if (!string.IsNullOrEmpty(filter) && RegisteredMessageFilter.TryGetValue(filter, out object filterFunction))
            {
                if (filterFunction is Func<HttpInformation, bool> filterFunctionNoParametersTemp)
                {
                    filterFunctionNoParameters = filterFunctionNoParametersTemp;
                }
                else if (filterFunction is Func<HttpInformation, object[], bool> filterFunctionParametersTemp)
                {
                    filterFunctionParameters = filterFunctionParametersTemp;
                }
            }

            Parallel.ForEach(connectionManager.connections.Values, connection =>
            {
                if (filterFunctionNoParameters != null && !filterFunctionNoParameters(connection.Information))
                {
                    return;
                }
                
                if (filterFunctionParameters != null && !filterFunctionParameters(connection.Information, filterParameters))
                {
                    return;
                }
                
                _ = connection.Send(new MessageResponse()
                {
                    Data = message
                });
            });
        }

        public void Publish(string topic, object message, bool retain, bool sync = true)
        {
            if (retain)
            {
                RetainedTopicMessages.AddOrUpdate(topic, message, ((s, o) => message));
            }
            
            if (sync)
            {
                syncManager.SendPublish(topic, message, retain);
            }

            Parallel.ForEach(connectionManager.connections.Values, connection =>
            {
                if (!connection.MessageSubscriptions.ContainsValue(topic))
                {
                    return;
                }

                foreach (KeyValuePair<string,string> subscription in connection.MessageSubscriptions.Where(s => s.Value == topic))
                {
                    _ = connection.Send(new TopicResponse()
                    {
                        ReferenceId = subscription.Key,
                        Message = message
                    });   
                }
            });
        }
    }
}
