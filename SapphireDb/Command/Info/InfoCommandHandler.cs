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
        private readonly IServiceProvider serviceProvider;

        public InfoCommandHandler(DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
            : base(dbContextAccessor)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<ResponseBase> Handle(HttpInformation context, InfoCommand command)
        {
            SapphireDbContext db = GetContext(command.ContextName);

            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);
            
            if (property.Key != null)
            {
                // if (!property.Key.CanQuery(context, serviceProvider))
                // {
                //     return Task.FromResult(
                //         command.CreateExceptionResponse<InfoResponse>(
                //             "Not allowed to query information for collection"));
                // }
                
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
