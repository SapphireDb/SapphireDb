using System;

namespace SapphireDb.Models.Exceptions
{
    public class UnhandledException : SapphireDbException
    {
        public UnhandledException(Exception exception) : base("An unhandled exception was thrown.", exception)
        {
            
        }
    }
}