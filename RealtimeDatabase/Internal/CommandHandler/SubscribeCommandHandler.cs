using System;
using System.Collections.Generic;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class SubscribeCommandHandler : CommandHandlerBase, ICommandHandler<SubscribeCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }
        private readonly IServiceProvider serviceProvider;

        public SubscribeCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, SubscribeCommand command)
        {
            CollectionSubscription collectionSubscription = new CollectionSubscription()
            {
                CollectionName = command.CollectionName.ToLowerInvariant(),
                ContextName = command.ContextName.ToLowerInvariant(),
                ReferenceId = command.ReferenceId,
                Prefilters = command.Prefilters
            };

            await Connection.AddSubscription(collectionSubscription);

            ResponseBase response = MessageHelper.GetCollection(GetContext(command.ContextName), command, context, serviceProvider, out List<object[]> transmittedData);

            await collectionSubscription.Lock.WaitAsync();
            collectionSubscription.TransmittedData = transmittedData;
            collectionSubscription.Lock.Release();

            return response;
        }
    }
}
