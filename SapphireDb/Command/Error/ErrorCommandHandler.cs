using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Error
{
    class ErrorCommandHandler : CommandHandlerBase, ICommandHandler<ErrorCommand>
    {
        public ErrorCommandHandler(DbContextAccesor contextAccessor) : base(contextAccessor)
        {
        }

        public Task<ResponseBase> Handle(IConnectionInformation context, ErrorCommand command, ExecutionContext executionContext)
        {
            throw command.Exception;
        }
    }
}