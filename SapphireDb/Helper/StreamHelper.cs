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
    public static class StreamHelper
    {
        public static bool closeStreamRunningInBackground = false;
        public static ConcurrentDictionary<Guid, StreamContainer> streamContainers = new ConcurrentDictionary<Guid, StreamContainer>();

        private static void CloseStream(Guid streamId)
        {
            streamContainers.TryRemove(streamId, out _);
            // ((IDisposable)pair.Value.AsyncEnumerable).Dispose();
        }
        
        public static void CloseOldStreamChannels()
        {
            if (!closeStreamRunningInBackground)
            {
                closeStreamRunningInBackground = true;
                Task.Run(async () =>
                {
                    while (streamContainers.Any())
                    {
                        streamContainers
                            .Where(c => c.Value.LastFrame < DateTime.UtcNow.AddMinutes(-10d))
                            .ToList()
                            .ForEach(pair => CloseStream(pair.Key));

                        Console.WriteLine("Run stream check");
                    
                        await Task.Delay(500);
                    }

                    closeStreamRunningInBackground = false;
                });
            }

        }
        
        public static void OpenStreamChannel(ConnectionBase connection, ExecuteCommand executeCommand, object asyncEnumerable)
        {
            CloseOldStreamChannels();
            
            Guid streamId = Guid.NewGuid();
            streamContainers.TryAdd(streamId, new StreamContainer()
            {
                ConnectionId = connection.Id,
                AsyncEnumerable = asyncEnumerable,
                LastFrame = DateTime.UtcNow,
                EnumerableType = asyncEnumerable.GetType()
            });

            _ = connection.Send(new InitStreamResponse()
            {
                ReferenceId = executeCommand.ReferenceId,
                Id = streamId
            });
        }

        public static void StreamData(Guid streamId, object frameData, ConnectionBase connection)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connection.Id)
            {
                streamContainer.EnumerableType
                    .GetMethod(nameof(AsyncEnumerable.Append))?
                    .Invoke(streamContainer.AsyncEnumerable, new [] {frameData});

            }
        }

        public static void CompleteStream(Guid streamId, ConnectionBase connection)
        {
            if (streamContainers.TryGetValue(streamId, out StreamContainer streamContainer) && streamContainer.ConnectionId == connection.Id)
            {
                CloseStream(streamId);
            }
        }
    }
}