using System;

namespace SapphireDb.Command
{
    public class CommandBase
    {
        public DateTime Timestamp { get; set; }
        
        public string ReferenceId { get; set; }
    }
}
