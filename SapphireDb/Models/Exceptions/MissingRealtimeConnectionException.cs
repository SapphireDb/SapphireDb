namespace SapphireDb.Models.Exceptions
{
    public class MissingRealtimeConnectionException : SapphireDbException
    {
        public string CommandType { get; }

        public MissingRealtimeConnectionException(string commandType) : base("Cannot handle this command without realtime connection")
        {
            CommandType = commandType;
        }
    }
}