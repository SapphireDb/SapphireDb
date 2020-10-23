namespace SapphireDb.Models.Exceptions
{
    public class IncludeNotAllowedException : SapphireDbException
    {
        public string ContextName { get; }
        
        public string CollectionName { get; }

        public IncludeNotAllowedException(string contextName, string collectionName) : base("Include prefilters are disabled")
        {
            ContextName = contextName;
            CollectionName = collectionName;
        }
    }
}