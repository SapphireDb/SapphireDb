using System;

namespace SapphireDb.Models
{
    public class SapphireDbError
    {
        public SapphireDbError(Exception exception)
        {
            Type = exception.GetType().Name;
            Message = exception.Message;
        }
        
        public string Type { get; }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}