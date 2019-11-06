using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection.Websocket;

namespace RealtimeDatabase.Connection.Poll
{
    public class PollConnection : ConnectionBase, IDisposable
    {
        public PollConnection(HttpContext context)
        {
            Init(context);
        }

        public override string Type => "Poll";

        public override async Task Send(object message)
        {
            await Lock.WaitAsync();

            try
            {
                //await Websocket.Send(message);
            }
            finally
            {
                Lock.Release();
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
    }
}
