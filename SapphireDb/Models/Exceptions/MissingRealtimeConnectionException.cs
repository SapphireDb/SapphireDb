using System;

namespace SapphireDb.Models.Exceptions
{
    public class MissingRealtimeConnectionException : Exception
    {
        public MissingRealtimeConnectionException() : base("Cannot handle this command without realtime connection")
        {
            
        }
    }
}