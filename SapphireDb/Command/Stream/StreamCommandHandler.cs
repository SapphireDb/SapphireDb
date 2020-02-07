using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Stream
{
    class StreamCommandHandler : CommandHandlerBase, ICommandHandler<StreamCommand>, INeedsConnection
    {
        private readonly SapphireStreamHelper streamHelper;
        public ConnectionBase Connection { get; set; }

        public StreamCommandHandler(DbContextAccesor dbContextAccessor, SapphireStreamHelper streamHelper)
            : base(dbContextAccessor)
        {
            this.streamHelper = streamHelper;
        }

        public Task<ResponseBase> Handle(HttpInformation context, StreamCommand command)
        {
            streamHelper.StreamData(command.StreamId, command.FrameData, command.Index, Connection.Id);
            return Task.FromResult<ResponseBase>(null);
        }
        
    }
}
