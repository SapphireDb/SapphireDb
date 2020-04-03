using System;

namespace SapphireDb.Models.Exceptions
{
    public class ValueNotFoundException : Exception
    {
        public ValueNotFoundException() : base("The value was not found")
        {
            
        }
    }
}