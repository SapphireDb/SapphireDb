using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Command.SubscribeMessage;
using SapphireDb.Command.Unsubscribe;
using SapphireDb.Command.UnsubscribeMessage;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public abstract class ConnectionBase : IDisposable
    {
        public void Init(HttpContext context)
        {
            Id = Guid.NewGuid();
            Subscriptions = new List<CollectionSubscription>();
            MessageSubscriptions = new Dictionary<string, string>();
            HttpContext = context;
            Information = new HttpInformation(context, Type);
        }

        public Guid Id { get; set; }
        
        public abstract string Type { get; }
        
        public HttpInformation Information { get; set; }

        public HttpContext HttpContext { get; set; }

        public List<CollectionSubscription> Subscriptions { get; set; }

        public Dictionary<string, string> MessageSubscriptions { get; set; }

        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);

        public abstract Task Send(object message);

        public abstract Task Close();

        public void Dispose()
        {
            Subscriptions.ForEach(s => s.Dispose());
        }

        public async Task AddSubscription(CollectionSubscription subscription)
        {
            await Lock.WaitAsync();

            try
            {
                if (Subscriptions.All(s => s.ReferenceId != subscription.ReferenceId))
                {
                    Subscriptions.Add(subscription);
                }
            }
            finally
            {
                Lock.Release();
            }
        }

        public async Task RemoveSubscription(UnsubscribeCommand command)
        {
            await Lock.WaitAsync();

            try
            {
                int index = Subscriptions.FindIndex(s => s.ReferenceId == command.ReferenceId);

                if (index != -1)
                {
                    Subscriptions.RemoveAt(index);
                }
            }
            finally
            {
                Lock.Release();
            }
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
