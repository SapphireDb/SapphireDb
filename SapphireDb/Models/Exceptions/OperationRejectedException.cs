namespace SapphireDb.Models.Exceptions
{
    public class OperationRejectedException : SapphireDbException
    {
        public OperationRejectedException(string message) : base(message)
        {
            
        }
    }
}