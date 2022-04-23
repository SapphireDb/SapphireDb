using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Sync.Models;

namespace SapphireDb.Sync
{
    public class SyncManager
    {
        private readonly ILogger<SyncManager> _logger;
        private readonly DbContextTypeContainer _contextTypeContainer;
        private readonly SyncContext _syncContext;
        private readonly ISapphireSyncModule _sapphireSyncModule;

        public SyncManager(IServiceProvider serviceProvider, ILogger<SyncManager> logger, DbContextTypeContainer contextTypeContainer, SyncContext syncContext)
        {
            _logger = logger;
            _contextTypeContainer = contextTypeContainer;
            _syncContext = syncContext;
            _sapphireSyncModule = (ISapphireSyncModule) serviceProvider.GetService(typeof(ISapphireSyncModule));

            if (_sapphireSyncModule != null)
            {
                _sapphireSyncModule.SyncRequestRequestReceived += request =>
                {
                    if (request.OriginId == syncContext.SessionId)
                    {
                        return;
                    }

                    if (request.Propagate)
                    {
                        Publish(request);
                    }

                    if (request is SendChangesRequest sendChangesRequest)
                    {
                        Type dbType = contextTypeContainer.GetContext(sendChangesRequest.DbName);

                        if (dbType != null)
                        {
                            SapphireChangeNotifier changeNotifier =
                                (SapphireChangeNotifier) serviceProvider.GetService(typeof(SapphireChangeNotifier));
                            logger.LogInformation("Handling changes from other server");
                            logger.LogDebug(
                                "Handling {ChangeCount} changes of {DbType} from server with OriginId {OriginId}. Propagate: {Propagate}",
                                sendChangesRequest.Changes.Count, dbType.Name,
                                sendChangesRequest.OriginId, sendChangesRequest.Propagate);

                            sendChangesRequest.Changes.ForEach(change =>
                            {
                                KeyValuePair<Type, string> property = dbType.GetDbSetType(change.CollectionName);
                                
                                JObject rawValue = change.Value as JObject;
                                change.Value = rawValue?.ToObject(property.Key);

                                if (change.State == ChangeResponse.ChangeState.Modified && change.OriginalValue != null)
                                {
                                    JObject rawOriginalValue = change.OriginalValue as JObject;
                                    change.OriginalValue = rawOriginalValue?.ToObject(property.Key);
                                }
                            });
                            
                            changeNotifier.HandleChanges(sendChangesRequest.Changes, dbType);
                        }
                    }
                    else if (request is SendMessageRequest sendMessageRequest)
                    {
                        SapphireMessageSender sender =
                            (SapphireMessageSender) serviceProvider.GetService(typeof(SapphireMessageSender));
                        logger.LogInformation("Handling message from other server");
                        logger.LogDebug(
                            "Handling message for filter {Filter} from server with OriginId {OriginId}. Propagate: {Propagate}",
                            sendMessageRequest.Filter, sendMessageRequest.OriginId, sendMessageRequest.Propagate);
                        
                        sender.Send(sendMessageRequest.Message, sendMessageRequest.Filter,
                            sendMessageRequest.FilterParameters, false);
                    }
                    else if (request is SendPublishRequest sendPublishRequest)
                    {
                        SapphireMessageSender sender =
                            (SapphireMessageSender) serviceProvider.GetService(typeof(SapphireMessageSender));
                        logger.LogInformation("Handling publish from other server");
                        logger.LogDebug(
                            "Handling publish to topic {Topic} from server with OriginId {OriginId}. Retain: {Retain}, Propagate: {Propagate}",
                            sendPublishRequest.Topic, sendPublishRequest.OriginId, sendPublishRequest.Retain, sendPublishRequest.Propagate);
                        
                        sender.Publish(sendPublishRequest.Topic, sendPublishRequest.Message, sendPublishRequest.Retain,
                            false);
                    }
                };
            }
        }

        public void SendChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            SendChangesRequest sendChangesRequest = new SendChangesRequest()
            {
                Changes = changes,
                DbName = _contextTypeContainer.GetName(dbContextType),
                OriginId = _syncContext.SessionId
            };

            Publish(sendChangesRequest);
        }

        public void SendPublish(string topic, object message, bool retain)
        {
            SendPublishRequest sendPublishRequest = new SendPublishRequest()
            {
                Topic = topic,
                Message = message,
                Retain = retain,
                OriginId = _syncContext.SessionId
            };

            Publish(sendPublishRequest);
        }

        public void SendMessage(object message, string filter, object[] filterParameters)
        {
            SendMessageRequest sendMessageRequest = new SendMessageRequest()
            {
                Message = message,
                Filter = filter,
                FilterParameters = filterParameters,
                OriginId = _syncContext.SessionId
            };

            Publish(sendMessageRequest);
        }

        private void Publish(SyncRequest syncRequest)
        {
            if (_sapphireSyncModule != null)
            {
                _logger.LogInformation("Publishing sync request to other servers");
                _sapphireSyncModule.Publish(syncRequest);
            }
        }
    }
}