using System;

namespace SapphireDb.Models.Exceptions
{
    public class ActionNotFoundException : Exception
    {
        public ActionNotFoundException() : base("No action to execute was found")
        {
            
        }
    }
}