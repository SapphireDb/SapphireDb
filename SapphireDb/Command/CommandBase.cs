using System;

namespace SapphireDb.Command
{
    public class CommandBase
    {
        public DateTimeOffset Timestamp { get; set; }
        
        public string ReferenceId { get; set; }
    }
}
