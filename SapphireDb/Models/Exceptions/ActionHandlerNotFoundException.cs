namespace SapphireDb.Models.Exceptions
{
    public class ActionHandlerNotFoundException : SapphireDbException
    {
        public string ActionHandler { get; }

        public ActionHandlerNotFoundException(string actionHandler) : base("No action handler was found for command")
        {
            ActionHandler = actionHandler;
        }
    }
}