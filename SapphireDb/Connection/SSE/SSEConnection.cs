using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Helper;

namespace SapphireDb.Connection.SSE
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
                await HttpContext.Response.Body.FlushAsync();
            }
            finally
            {
                Lock.Release();
            }
        }

        public override Task Close()
        {
            HttpContext.Abort();
            return Task.CompletedTask;
        }
    }
}
