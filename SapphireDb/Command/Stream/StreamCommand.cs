using System;
using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.Stream
{
    public class StreamCommand : CommandBase
    {
        public Guid StreamId { get; set; }
        
        public JToken FrameData { get; set; }

        public int Index { get; set; }
    }
}