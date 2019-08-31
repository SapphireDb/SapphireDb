using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Connection.SSE
{
    public class SSEConnection : ConnectionBase
    {
        public SSEConnection(HttpContext context)
        {
            Init(context);
        }

        public override string Type => "SSE";

        public override async Task Send(object message)
        {
            await Lock.WaitAsync();

            try
            {
                string messageString = JsonHelper.Serialize(message);
                await HttpContext.Response.WriteAsync($"data: {messageString}\n\n");
                HttpContext.Response.Body.Flush();
            }
            finally
            {
                Lock.Release();
            }
        }

        public override Task Close()
        {
            throw new NotImplementedException();
        }
    }
}
