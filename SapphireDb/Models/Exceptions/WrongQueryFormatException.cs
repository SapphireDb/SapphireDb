namespace SapphireDb.Models.Exceptions
{
    public class WrongQueryFormatException : SapphireDbException
    {
        public string Query { get; }

        public WrongQueryFormatException(string query) : base("Wrong format of query name.")
        {
            Query = query;
        }
    }
}