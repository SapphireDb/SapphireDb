using System;

namespace SapphireDb.Models.Exceptions
{
    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException() : base("No handler was found for command")
        {
            
        }
    }
}