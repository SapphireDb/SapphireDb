using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;

namespace RealtimeDatabase.Connection
{
    [DataContract]
    public abstract class ConnectionBase : IDisposable
    {
        public void Init(HttpContext context)
        {
            Id = Guid.NewGuid();
            Subscriptions = new List<CollectionSubscription>();
            MessageSubscriptions = new Dictionary<string, string>();
            HttpContext = context;

            if (HttpContext.Request.Query.TryGetValue("key", out StringValues apiKey))
            {
                RealtimeDatabaseOptions options = context.RequestServices.GetService<RealtimeDatabaseOptions>();
                ApiName = options.ApiConfigurations.FirstOrDefault(c => c.Key == apiKey.ToString())?.Name;
            }

            if (HttpContext.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent))
            {
                UserAgent = userAgent.ToString();
            }

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                UserId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            }
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string UserAgent { get; set; }

        [DataMember]
        public abstract string Type { get; }

        [DataMember]
        public string ApiName { get; set; }

        public HttpContext HttpContext { get; set; }

        public List<CollectionSubscription> Subscriptions { get; set; }

        public Dictionary<string, string> MessageSubscriptions { get; set; }

        public string UsersSubscription { get; set; }

        public string RolesSubscription { get; set; }

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
                Subscriptions.Add(subscription);
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

        public async Task AddRolesSubscription(SubscribeRolesCommand command)
        {
            await Lock.WaitAsync();

            try
            {
                RolesSubscription = command.ReferenceId;
            }
            finally
            {
                Lock.Release();
            }
        }

        public async Task RemoveRolesSubscription()
        {
            await Lock.WaitAsync();

            try
            {
                RolesSubscription = null;
            }
            finally
            {
                Lock.Release();
            }
        }

        public async Task AddUsersSubscription(SubscribeUsersCommand command)
        {
            await Lock.WaitAsync();

            try
            {
                UsersSubscription = command.ReferenceId;
            }
            finally
            {
                Lock.Release();
            }
        }

        public async Task RemoveUsersSubscription()
        {
            await Lock.WaitAsync();

            try
            {
                UsersSubscription = null;
            }
            finally
            {
                Lock.Release();
            }
        }
    }
}
