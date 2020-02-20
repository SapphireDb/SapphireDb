using System.Collections.Generic;

namespace SapphireDb.Command.ExecuteCommands
{
    public class ExecuteCommandsCommand : CommandBase
    {
        public List<CommandBase> Commands { get; set; }
    }
}