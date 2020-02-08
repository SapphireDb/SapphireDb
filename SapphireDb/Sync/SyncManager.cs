using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Sync.Models;

namespace SapphireDb.Sync
{
    public class SyncManager
    {
        private readonly Guid uId = Guid.NewGuid();
        private readonly ISapphireSyncModule sapphireSyncModule;

        public SyncManager(IServiceProvider serviceProvider, ILogger<SyncManager> logger)
        {
            sapphireSyncModule = (ISapphireSyncModule) serviceProvider.GetService(typeof(ISapphireSyncModule));

            if (sapphireSyncModule != null)
            {
                sapphireSyncModule.SyncRequestRequestReceived += request =>
                {
                    if (request.OriginId == uId)
                    {
                        return;
                    }
                    
                    if (request.Propagate)
                    {
                        Publish(request);
                    }

                    if (request is SendChangesRequest sendChangesRequest)
                    {
                        Type dbType = Assembly.GetEntryAssembly()?.DefinedTypes
                            .FirstOrDefault(t => t.FullName == sendChangesRequest.DbType);

                        if (dbType != null)
                        {
                            SapphireChangeNotifier changeNotifier = (SapphireChangeNotifier) serviceProvider.GetService(typeof(SapphireChangeNotifier));
                            logger.LogInformation("Handling changes from other server");
                            changeNotifier.HandleChanges(sendChangesRequest.Changes, dbType);
                        }
                    }
                    else if (request is SendMessageRequest sendMessageRequest)
                    {
                        SapphireMessageSender sender = (SapphireMessageSender) serviceProvider.GetService(typeof(SapphireMessageSender));
                        logger.LogInformation("Handling message from other server");
                        sender.Send(sendMessageRequest.Message, sendMessageRequest.Filter, sendMessageRequest.FilterParameters, false);
                    }
                    else if (request is SendPublishRequest sendPublishRequest)
                    {
                        SapphireMessageSender sender = (SapphireMessageSender) serviceProvider.GetService(typeof(SapphireMessageSender));
                        logger.LogInformation("Handling publish from other server");
                        sender.Publish(sendPublishRequest.Topic, sendPublishRequest.Message, sendPublishRequest.Retain, false);
                    }
                };
            }
        }

        public void SendChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            SendChangesRequest sendChangesRequest = new SendChangesRequest()
            {
                Changes = changes,
                DbType = dbContextType.FullName,
                OriginId = uId
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
                OriginId = uId
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
                OriginId = uId
            };

            Publish(sendMessageRequest);
        }

        private void Publish(SyncRequest syncRequest)
        {
            sapphireSyncModule?.Publish(syncRequest);
        }
    }
}