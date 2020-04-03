using System;

namespace SapphireDb.Models.Exceptions
{
    public class ActionHandlerNotFoundException : Exception
    {
        public ActionHandlerNotFoundException() : base("No action handler was found for command")
        {
            
        }
    }
}