using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Connection.Websocket;
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

        private readonly ConcurrentQueue<object> messages = new ConcurrentQueue<object>();
        public DateTime lastPoll;

        public override string Type => "Poll";

        public override Task Send(object message)
        {
            messages.Enqueue(message);
            return Task.CompletedTask;
        }

        public IEnumerable<object> GetMessages()
        {
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
