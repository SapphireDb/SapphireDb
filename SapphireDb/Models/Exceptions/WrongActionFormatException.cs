namespace SapphireDb.Models.Exceptions
{
    public class WrongActionFormatException : SapphireDbException
    {
        public string Action { get; }

        public WrongActionFormatException(string action) : base("Wrong format of action name.")
        {
            Action = action;
        }
    }
}