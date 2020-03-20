using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Command;
using SapphireDb.Command.SubscribeMessage;
using SapphireDb.Command.UnsubscribeMessage;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public abstract class ConnectionBase : IDisposable
    {
        public void Init(HttpContext context)
        {
            Id = Guid.NewGuid();
            MessageSubscriptions = new Dictionary<string, string>();
            HttpContext = context;
            Information = new HttpInformation(context, Type);
        }

        public Guid Id { get; set; }
        
        public abstract string Type { get; }
        
        public HttpInformation Information { get; set; }

        public HttpContext HttpContext { get; set; }
        
        public Dictionary<string, string> MessageSubscriptions { get; set; }

        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);

        public abstract Task Send(ResponseBase message);

        public abstract Task Close();

        public void Dispose()
        {
            
        }

        public async Task AddMessageSubscription(SubscribeMessageCommand command)
        {
            await Lock.WaitAsync();

            try
            {
                MessageSubscriptions.Add(command.ReferenceId, command.Topic);
            }
            finally
            {
                Lock.Release();
            }
        }

        public async Task RemoveMessageSubscription(UnsubscribeMessageCommand command)
        {
            await Lock.WaitAsync();

            try
            {
                MessageSubscriptions.Remove(command.ReferenceId);
            }
            finally
            {
                Lock.Release();
            }
        }
    }
}
