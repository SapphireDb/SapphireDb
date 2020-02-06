using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Stream
{
    class CompleteStreamCommandHandler : CommandHandlerBase, ICommandHandler<CompleteStreamCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public CompleteStreamCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {
        }

        public Task<ResponseBase> Handle(HttpInformation context, CompleteStreamCommand command)
        {
            StreamHelper.CompleteStream(command.StreamId, Connection);
            return Task.FromResult<ResponseBase>(null);
        }
        
    }
}
