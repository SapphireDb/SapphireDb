using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class InfoCommandHandler : CommandHandlerBase, ICommandHandler<InfoCommand>
    {
        public InfoCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public Task<ResponseBase> Handle(HttpContext context, InfoCommand command)
        {
            RealtimeDbContext db = GetContext();

            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                InfoResponse infoResponse = property.Key.GetInfoResponse(db);
                infoResponse.ReferenceId = command.ReferenceId;
                return Task.FromResult<ResponseBase>(infoResponse);
            }

            return Task.FromResult(command.CreateExceptionResponse<InfoResponse>("No set for collection was found."));
        }
    }
}
