namespace SapphireDb.Models.Exceptions
{
    public class ActionNotFoundException : SapphireDbException
    {
        public string ActionHandler { get; }
        
        public string Action { get; }

        public ActionNotFoundException(string actionHandler, string action) : base("No action to execute was found")
        {
            ActionHandler = actionHandler;
            Action = action;
        }
    }
}