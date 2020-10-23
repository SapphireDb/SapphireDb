namespace SapphireDb.Models.Exceptions
{
    public class CollectionNotFoundException : SapphireDbException
    {
        public string ContextName { get; }
        
        public string Collection { get; }

        public CollectionNotFoundException(string contextName, string collection) : base("No collection was found for given collection name")
        {
            ContextName = contextName;
            Collection = collection;
        }
    }
}