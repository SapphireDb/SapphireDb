namespace SapphireDb.Models.Exceptions
{
    public class IncludeNotAllowedException : SapphireDbException
    {
        public string CollectionName { get; }

        public IncludeNotAllowedException(string collectionName) : base("Include prefilters are disabled")
        {
            CollectionName = collectionName;
        }
    }
}