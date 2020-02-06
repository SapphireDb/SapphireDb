using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Stream
{
    class StreamCommandHandler : CommandHandlerBase, ICommandHandler<StreamCommand>, INeedsConnection
    {
        public ConnectionBase Connection { get; set; }

        public StreamCommandHandler(DbContextAccesor dbContextAccessor)
            : base(dbContextAccessor)
        {
        }

        public Task<ResponseBase> Handle(HttpInformation context, StreamCommand command)
        {
            StreamHelper.StreamData(command.StreamId, command.FrameData, Connection);
            return Task.FromResult<ResponseBase>(null);
        }
        
    }
}
