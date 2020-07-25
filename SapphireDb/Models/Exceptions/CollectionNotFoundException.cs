namespace SapphireDb.Models.Exceptions
{
    public class CollectionNotFoundException : SapphireDbException
    {
        public string Collection { get; }

        public CollectionNotFoundException(string collection) : base("No collection was found for given collection name")
        {
            Collection = collection;
        }
    }
}