using System;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class QueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryCommand>, IRestFallback
    {
        private readonly IServiceProvider serviceProvider;

        public QueryCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpContext context, QueryCommand command)
        {
            return Task.FromResult(MessageHelper.GetCollection(GetContext(), command, context, serviceProvider, out _));
        }
    }
}
