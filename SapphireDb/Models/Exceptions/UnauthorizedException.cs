namespace SapphireDb.Models.Exceptions
{
    public class UnauthorizedException : SapphireDbException
    {
        public UnauthorizedException(string message) : base(message)
        {
            
        }
    }
}