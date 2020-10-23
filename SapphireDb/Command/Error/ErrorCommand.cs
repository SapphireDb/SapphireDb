using System;

namespace SapphireDb.Command.Error
{
    public class ErrorCommand : CommandBase
    {
        public Exception Exception { get; set; }
    }
}