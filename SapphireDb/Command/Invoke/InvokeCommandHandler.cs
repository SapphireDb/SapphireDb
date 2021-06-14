using System;
using System.Threading.Tasks;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Invoke
{
    class InvokeCommandHandler : CommandHandlerBase, ICommandHandler<InvokeCommand>
    {
        public InvokeCommandHandler(DbContextAccesor contextAccessor) : base(contextAccessor)
        {
        }

        public Task<ResponseBase> Handle(HttpInformation context, InvokeCommand command, ExecutionContext executionContext)
        {
            throw new NotImplementedException();
        }
    }
}