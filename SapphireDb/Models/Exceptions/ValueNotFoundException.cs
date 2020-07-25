namespace SapphireDb.Models.Exceptions
{
    public class ValueNotFoundException : SapphireDbException
    {
        public string CollectionName { get; }
        
        public object[] PrimaryKeys { get; }

        public ValueNotFoundException(string collectionName, object[] primaryKeys) : base("The value was not found")
        {
            CollectionName = collectionName;
            PrimaryKeys = primaryKeys;
        }
    }
}