namespace SapphireDb.Models.Exceptions
{
    public class ActionNotFoundException : SapphireDbException
    {
        public string Action { get; }

        public ActionNotFoundException(string action) : base("No action to execute was found")
        {
            Action = action;
        }
    }
}