using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Info
{
    class InfoCommandHandler : CommandHandlerBase, ICommandHandler<InfoCommand>
    {
        public InfoCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {

        }

        public Task<ResponseBase> Handle(HttpInformation context, InfoCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);

            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key != null)
            {
                InfoResponse infoResponse = new InfoResponse
                {
                    PrimaryKeys = property.Key.GetPrimaryKeyNames(db),
                    ReferenceId = command.ReferenceId
                };

                return Task.FromResult<ResponseBase>(infoResponse);
            }

            return Task.FromResult(command.CreateExceptionResponse<InfoResponse>("No set for collection was found."));
        }
    }
}
