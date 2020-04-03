using System;

namespace SapphireDb.Models.Exceptions
{
    public class OperationRejectedException : Exception
    {
        public OperationRejectedException(string message) : base(message)
        {
            
        }
    }
}