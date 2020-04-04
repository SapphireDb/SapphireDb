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
        public static readonly Dictionary<string, object> RegisteredMessageFilter = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        
        private readonly ConnectionManager connectionManager;
        private readonly SyncManager syncManager;
        private readonly MessageSubscriptionManager messageSubscriptionManager;

        public SapphireMessageSender(ConnectionManager connectionManager, SyncManager syncManager,
            MessageSubscriptionManager messageSubscriptionManager)
        {
            this.connectionManager = connectionManager;
            this.syncManager = syncManager;
            this.messageSubscriptionManager = messageSubscriptionManager;
        }

        public void Send(object message, string filter = null, object[] filterParameters = null, bool sync = true)
        {
            Task.Run(() =>
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
                    Task.Run(() =>
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
                });
            });
        }

        public void Publish(string topic, object message, bool retain, bool sync = true)
        {
            Task.Run(() =>
            {
                if (retain)
                {
                    RetainedTopicMessages.AddOrUpdate(topic, message, ((s, o) => message));
                }
            
                if (sync)
                {
                    syncManager.SendPublish(topic, message, retain);
                }

                List<Subscription> topicSubscriptions = messageSubscriptionManager.GetTopicSubscriptions(topic);

                if (topicSubscriptions == null)
                {
                    return;
                }
            
                Parallel.ForEach(topicSubscriptions, subscription =>
                {
                    Task.Run(() =>
                    {
                        _ = subscription.Connection.Send(new TopicResponse()
                        {
                            ReferenceId = subscription.ReferenceId,
                            Message = message,
                            Topic = topic
                        });     
                    });
                });
            });
        }
    }
}
