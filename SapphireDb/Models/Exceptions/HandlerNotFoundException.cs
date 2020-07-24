namespace SapphireDb.Models.Exceptions
{
    public class HandlerNotFoundException : SapphireDbException
    {
        public HandlerNotFoundException() : base("No handler was found for command")
        {
            
        }
    }
}