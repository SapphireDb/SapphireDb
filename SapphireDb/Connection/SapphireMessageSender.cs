using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SapphireDb.Command.Message;
using SapphireDb.Command.SubscribeMessage;
using SapphireDb.Models;
using SapphireDb.Sync;

namespace SapphireDb.Connection
{
    public class SapphireMessageSender
    {
        public static readonly ConcurrentDictionary<string, object> RetainedTopicMessages = new ConcurrentDictionary<string, object>();
        public static readonly Dictionary<string, object> RegisteredMessageFilter = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        
        private readonly ConnectionManager _connectionManager;
        private readonly SyncManager _syncManager;
        private readonly MessageSubscriptionManager _messageSubscriptionManager;
        private readonly IServiceProvider _serviceProvider;

        public SapphireMessageSender(ConnectionManager connectionManager, SyncManager syncManager,
            MessageSubscriptionManager messageSubscriptionManager, IServiceProvider serviceProvider)
        {
            _connectionManager = connectionManager;
            _syncManager = syncManager;
            _messageSubscriptionManager = messageSubscriptionManager;
            _serviceProvider = serviceProvider;
        }

        public void Send(object message, string filter = null, object[] filterParameters = null, bool sync = true)
        {
            Task.Run(() =>
            {
                if (sync)
                {
                    _syncManager.SendMessage(message, filter, filterParameters);
                }
            
                Func<IConnectionInformation, bool> filterFunctionNoParameters = null;
                Func<IConnectionInformation, object[], bool> filterFunctionParameters = null;
            
                if (!string.IsNullOrEmpty(filter) && RegisteredMessageFilter.TryGetValue(filter, out object filterFunction))
                {
                    if (filterFunction is Func<IConnectionInformation, bool> filterFunctionNoParametersTemp)
                    {
                        filterFunctionNoParameters = filterFunctionNoParametersTemp;
                    }
                    else if (filterFunction is Func<IConnectionInformation, object[], bool> filterFunctionParametersTemp)
                    {
                        filterFunctionParameters = filterFunctionParametersTemp;
                    }
                }

                Parallel.ForEach(_connectionManager.connections.Values, connection =>
                {
                    Task.Run(() =>
                    {
                        if (filterFunctionNoParameters != null && !filterFunctionNoParameters(connection))
                        {
                            return;
                        }
                
                        if (filterFunctionParameters != null && !filterFunctionParameters(connection, filterParameters))
                        {
                            return;
                        }
                
                        _ = connection.Send(new MessageResponse()
                        {
                            Data = message
                        }, _serviceProvider);
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
                    _syncManager.SendPublish(topic, message, retain);
                }

                List<Subscription> topicSubscriptions = _messageSubscriptionManager.GetTopicSubscriptions(topic);

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
                        }, _serviceProvider);     
                    });
                });
            });
        }
    }
}
