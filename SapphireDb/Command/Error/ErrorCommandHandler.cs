using System.Threading.Tasks;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Error
{
    class ErrorCommandHandler : CommandHandlerBase, ICommandHandler<ErrorCommand>
    {
        public ErrorCommandHandler(DbContextAccesor contextAccessor) : base(contextAccessor)
        {
        }

        public Task<ResponseBase> Handle(HttpInformation context, ErrorCommand command, ExecutionContext executionContext)
        {
            throw command.Exception;
        }
    }
}