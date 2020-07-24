namespace SapphireDb.Models.Exceptions
{
    public class ActionHandlerNotFoundException : SapphireDbException
    {
        public ActionHandlerNotFoundException() : base("No action handler was found for command")
        {
            
        }
    }
}