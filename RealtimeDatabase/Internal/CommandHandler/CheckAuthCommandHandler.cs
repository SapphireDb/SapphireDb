using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CheckAuthCommandHandler : CommandHandlerBase, ICommandHandler<CheckAuthCommand>
    {
        public CheckAuthCommandHandler(DbContextAccesor contextAccessor) : base(contextAccessor)
        {
        }

        public Task<ResponseBase> Handle(HttpContext context, CheckAuthCommand command)
        {
            return Task.FromResult<ResponseBase>(new CheckAuthResponse()
            {
                ReferenceId = command.ReferenceId,
                Authenticated = context.User.Identity.IsAuthenticated
            });
        }
    }
}
