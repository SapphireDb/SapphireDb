using System.Collections.Generic;

namespace SapphireDb.Command.ExecuteCommands
{
    public class ExecuteCommandsResponse : ResponseBase
    {
        public List<ExecuteCommandsResultResponse> Results { get; set; }
    }

    public class ExecuteCommandsResultResponse : ResponseBase
    {
        public CommandBase Command { get; set; }
        
        public ResponseBase Response { get; set; }
    }
}