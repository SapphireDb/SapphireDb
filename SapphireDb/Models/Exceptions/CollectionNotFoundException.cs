namespace SapphireDb.Models.Exceptions
{
    public class CollectionNotFoundException : SapphireDbException
    {
        public CollectionNotFoundException() : base("No collection was found for given collection name")
        {
            
        }
    }
}