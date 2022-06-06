namespace SapphireDb.Models.Exceptions;

public class PrimaryKeyValuesMissingException : SapphireDbException
{
    public string[] PrimaryKeys { get; }
    
    public PrimaryKeyValuesMissingException(string[] primaryKeys) : base("Primary key values missing in data")
    {
        PrimaryKeys = primaryKeys;
    }
}