using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.Stream
{
    class CompleteStreamCommandHandler : CommandHandlerBase, ICommandHandler<CompleteStreamCommand>, INeedsConnection
    {
        private readonly SapphireStreamHelper streamHelper;
        public ConnectionBase Connection { get; set; }

        public CompleteStreamCommandHandler(DbContextAccesor dbContextAccessor, SapphireStreamHelper streamHelper)
            : base(dbContextAccessor)
        {
            this.streamHelper = streamHelper;
        }

        public Task<ResponseBase> Handle(HttpInformation context, CompleteStreamCommand command)
        {
            streamHelper.CompleteStream(command.StreamId, command.Index, command.Error, Connection.Id);
            return Task.FromResult<ResponseBase>(null);
        }
        
    }
}
