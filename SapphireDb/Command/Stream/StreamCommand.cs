using System;

namespace SapphireDb.Command.Stream
{
    public class StreamCommand : CommandBase
    {
        public Guid StreamId { get; set; }
        
        public object FrameData { get; set; }
    }
}