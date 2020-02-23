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
        public DateTime lastPoll;

        public override string Type => "Poll";

        private SemaphoreSlim MessageLock = new SemaphoreSlim(0, 1);
        
        public override Task Send(ResponseBase message)
        {
            messages.Enqueue(message);
            try { MessageLock.Release(); } catch (SemaphoreFullException) { }
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<object>> GetMessages()
        {
            await MessageLock.WaitAsync();
            lastPoll = DateTime.UtcNow;
            return messages.DequeueChunk();
        }

        public override Task Close()
        {
            return Task.CompletedTask;
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
