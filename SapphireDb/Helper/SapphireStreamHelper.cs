using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Command.Execute;
using SapphireDb.Command.Stream;
using SapphireDb.Connection;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    public class SapphireStreamHelper
    {
        public readonly ConcurrentDictionary<Guid, StreamContainer> streamContainers = new ConcurrentDictionary<Guid, StreamContainer>();

        private void CloseStream(Guid streamId)
        {
            streamContainers.TryRemove(streamId, out StreamContainer streamContainer);
            streamContainer.Abort();
        }
        
        public void CloseOldStreamChannels()
        {
            streamContainers
                .Where(c => c.Value.LastFrame < DateTime.UtcNow.AddMinutes(-10d))
                .ToList()
                .ForEach(pair => CloseStream(pair.Key));
        }
        
        public object OpenStreamChannel(ConnectionBase connection, ExecuteCommand executeCommand, Type parameterType)
        {
            CloseOldStreamChannels();

            StreamContainer streamContainer = new StreamContainer(connection.Id, parameterType);
            
            Guid streamId = Guid.NewGuid();
            streamContainers.TryAdd(streamId, streamContainer);

            _ = connection.Send(new InitStreamResponse()
            {
                ReferenceId = executeCommand.ReferenceId,
                Id = streamId
            });

            return streamContainer.AsyncEnumerableValue;
        }

        public void StreamData(Guid streamId, object frameData, int index, Guid connectionId)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connectionId)
            {
                streamContainer.NewValue(frameData, index);
            }
        }

        public void CompleteStream(Guid streamId, int index, Guid connectionId)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connectionId)
            {
                streamContainer.Complete(index);
                CloseStream(streamId);
            }
        }
    }
}