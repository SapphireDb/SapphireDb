using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using SapphireDb.Command.Execute;
using SapphireDb.Command.Stream;
using SapphireDb.Connection;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    public class SapphireStreamHelper
    {
        public bool closeStreamRunningInBackground = false;
        public readonly ConcurrentDictionary<Guid, StreamContainer> streamContainers = new ConcurrentDictionary<Guid, StreamContainer>();

        public void CloseOldStreamChannels()
        {
            if (!closeStreamRunningInBackground)
            {
                closeStreamRunningInBackground = true;
                Task.Run(async () =>
                {
                    while (streamContainers.Any())
                    {
                        streamContainers
                            .Where(c => c.Value.LastFrame < DateTimeOffset.UtcNow.AddMinutes(-1d))
                            .ToList()
                            .ForEach(pair =>
                            {
                                streamContainers.TryRemove(pair.Key, out StreamContainer streamContainer);
                                streamContainer.Abort();
                            });
                    
                        await Task.Delay(500);
                    }

                    closeStreamRunningInBackground = false;
                });
            }
        }
        
        public object OpenStreamChannel(SignalRConnection connection, ExecuteCommand executeCommand, Type parameterType,
            IServiceProvider serviceProvider)
        {
            StreamContainer streamContainer = new StreamContainer(connection.Id, parameterType);
            
            Guid streamId = Guid.NewGuid();
            streamContainers.TryAdd(streamId, streamContainer);

            _ = connection.Send(new InitStreamResponse()
            {
                ReferenceId = executeCommand.ReferenceId,
                Id = streamId
            }, serviceProvider);

            CloseOldStreamChannels();
            
            return streamContainer.AsyncEnumerableValue;
        }

        public void StreamData(Guid streamId, JToken frameData, int index, string connectionId)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connectionId)
            {
                streamContainer.NewValue(frameData, index);
            }
        }

        public void CompleteStream(Guid streamId, int index, bool error, string connectionId)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connectionId)
            {
                streamContainer.Complete(index, error);
            }
        }
    }
}