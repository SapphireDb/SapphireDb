using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Command;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public abstract class ConnectionBase : IDisposable
    {
        public void Init(HttpContext context)
        {
            Id = Guid.NewGuid();
            HttpContext = context;
            Information = new HttpInformation(context, Type);
        }

        public Guid Id { get; set; }
        
        public abstract string Type { get; }
        
        public HttpInformation Information { get; set; }

        public HttpContext HttpContext { get; set; }

        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);

        public abstract Task Send(ResponseBase message);

        public abstract Task Close();

        public void Dispose()
        {
            
        }
    }
}
