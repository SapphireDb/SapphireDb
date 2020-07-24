namespace SapphireDb.Models.Exceptions
{
    public class MissingRealtimeConnectionException : SapphireDbException
    {
        public MissingRealtimeConnectionException() : base("Cannot handle this command without realtime connection")
        {
            
        }
    }
}