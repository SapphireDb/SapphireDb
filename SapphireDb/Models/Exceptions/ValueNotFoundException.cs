namespace SapphireDb.Models.Exceptions
{
    public class ValueNotFoundException : SapphireDbException
    {
        public string ContextName { get; set; }
        
        public string CollectionName { get; }
        
        public object[] PrimaryKeys { get; }

        public ValueNotFoundException(string contextName, string collectionName, object[] primaryKeys) : base("The value was not found")
        {
            ContextName = contextName;
            CollectionName = collectionName;
            PrimaryKeys = primaryKeys;
        }
    }
}