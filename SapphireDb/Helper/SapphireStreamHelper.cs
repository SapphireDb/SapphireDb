using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                            .Where(c => c.Value.LastFrame < DateTime.UtcNow.AddMinutes(-1d))
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
        
        public object OpenStreamChannel(ConnectionBase connection, ExecuteCommand executeCommand, Type parameterType)
        {
            StreamContainer streamContainer = new StreamContainer(connection.Id, parameterType);
            
            Guid streamId = Guid.NewGuid();
            streamContainers.TryAdd(streamId, streamContainer);

            _ = connection.Send(new InitStreamResponse()
            {
                ReferenceId = executeCommand.ReferenceId,
                Id = streamId
            });

            CloseOldStreamChannels();
            
            return streamContainer.AsyncEnumerableValue;
        }

        public void StreamData(Guid streamId, JToken frameData, int index, Guid connectionId)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connectionId)
            {
                streamContainer.NewValue(frameData, index);
            }
        }

        public void CompleteStream(Guid streamId, int index, bool error, Guid connectionId)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connectionId)
            {
                streamContainer.Complete(index, error);
            }
        }
    }
}