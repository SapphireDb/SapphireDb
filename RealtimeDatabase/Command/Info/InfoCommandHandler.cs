using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.Info
{
    class InfoCommandHandler : CommandHandlerBase, ICommandHandler<InfoCommand>
    {
        public InfoCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public Task<ResponseBase> Handle(HttpInformation context, InfoCommand command)
        {
            RealtimeDbContext db = GetContext(command.ContextName);

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
