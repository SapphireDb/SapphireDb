using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Command;
using SapphireDb.Helper;

namespace SapphireDb.Connection.Poll
{
    public class PollConnection : ConnectionBase, IDisposable
    {
        public PollConnection(HttpContext context)
        {
            Init(context);
            lastPoll = DateTime.UtcNow;
            HttpContext = null;
        }

        private readonly ConcurrentQueue<ResponseBase> messages = new ConcurrentQueue<ResponseBase>();

        public bool pollInProgress;
        public DateTime lastPoll;

        public override string Type => "Poll";

        private readonly SemaphoreSlim messageLock = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim lastPollLock = new SemaphoreSlim(1, 1);
        
        public override Task Send(ResponseBase message)
        {
            messages.Enqueue(message);
            try { messageLock.Release(); } catch (SemaphoreFullException) { }
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<object>> GetMessages(CancellationToken requestAborted)
        {
            try
            {
                try
                {
                    lastPollLock.Wait();
                    pollInProgress = true;
                }
                finally
                {
                    lastPollLock.Release();
                }
                
                await messageLock.WaitAsync(requestAborted);
                return messages.DequeueChunk();
            }
            finally
            {
                try
                {
                    lastPollLock.Wait();
                    pollInProgress = false;
                    lastPoll = DateTime.UtcNow;
                }
                finally
                {
                    lastPollLock.Release();
                }
            }
        }

        public override Task Close()
        {
            return Task.CompletedTask;
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        public bool ShouldRemove()
        {
            try
            {
                lastPollLock.Wait();
                return !pollInProgress && lastPoll < DateTime.UtcNow.AddSeconds(-5d);
            }
            finally
            {
                lastPollLock.Release();
            }
        }
    }
}
