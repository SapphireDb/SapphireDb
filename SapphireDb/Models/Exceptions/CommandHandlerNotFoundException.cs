namespace SapphireDb.Models.Exceptions
{
    public class CommandHandlerNotFoundException : SapphireDbException
    {
        public string CommandType { get; }

        public CommandHandlerNotFoundException(string commandType) : base("No handler was found for command")
        {
            CommandType = commandType;
        }
    }
}