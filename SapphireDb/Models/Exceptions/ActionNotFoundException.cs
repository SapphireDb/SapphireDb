namespace SapphireDb.Models.Exceptions
{
    public class ActionNotFoundException : SapphireDbException
    {
        public ActionNotFoundException() : base("No action to execute was found")
        {
            
        }
    }
}