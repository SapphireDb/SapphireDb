using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SapphireDb.Helper
{
    static class QueueHelper
    {
        public static IEnumerable<T> DequeueChunk<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out T result))
            {
                yield return result;
            }
        }
    }
}
