using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace SapphireDb.Models
{
    public class StreamContainer
    {
        public StreamContainer(string connectionId, Type parameterType)
        {
            ConnectionId = connectionId;
            EnumerableGenericType = parameterType.GenericTypeArguments.FirstOrDefault();
            LastFrame = DateTimeOffset.UtcNow;
            AsyncEnumerableValue = CreateAsyncEnumerableWrapper();
        }
        
        public string ConnectionId { get; set; }

        public DateTimeOffset LastFrame { get; set; }

        private Type EnumerableGenericType { get; set; }
        
        public object AsyncEnumerableValue { get; set; }

        private object CreateAsyncEnumerableWrapper()
        {
            return GetType()
                .GetMethod(nameof(CreateAsyncEnumerable), BindingFlags.Instance|BindingFlags.NonPublic)?
                .MakeGenericMethod(EnumerableGenericType)
                .Invoke(this, new object [] { });
        }

        private readonly AutoResetEvent newData = new AutoResetEvent(false);
        private readonly ConcurrentDictionary<int, StreamFrame> streamFrames = new ConcurrentDictionary<int, StreamFrame>();

        private bool aborted = false;
        
#pragma warning disable 1998
        private async IAsyncEnumerable<T> CreateAsyncEnumerable<T>()
#pragma warning restore 1998
        {
            int currentIndex = 0;

            newData.WaitOne();

            while (!aborted)
            {
                if (streamFrames.TryGetValue(currentIndex, out StreamFrame streamFrame))
                {
                    if (streamFrame is StreamData streamData)
                    {
                        yield return (T) streamData.Data;
                    }
                    else if (streamFrame is StreamEnd streamEnd)
                    {
                        if (streamEnd.Error)
                        {
                            throw new Exception();
                        }
                        
                        break;
                    }

                    currentIndex++;
                }
                else
                {
                    newData.WaitOne();
                }
            }

            if (aborted)
            {
                throw new TimeoutException();
            }
        }

        public void NewValue(JToken value, int index)
        {
            object valueObject = value?.ToObject(EnumerableGenericType);

            LastFrame = DateTimeOffset.UtcNow;
            streamFrames.TryAdd(index, new StreamData() { Data = valueObject });
            newData.Set();
        }
        
        public void Complete(int index, bool error)
        {
            streamFrames.TryAdd(index, new StreamEnd() { Error = error });
            newData.Set();
        }

        public void Abort()
        {
            aborted = true;
            newData.Set();
        }
    }
    
    class StreamFrame { }

    class StreamData : StreamFrame
    {
        public object Data { get; set; }
    }

    class StreamEnd : StreamFrame
    {
        public bool Error { get; set; }
    }
}