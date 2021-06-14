namespace SapphireDb.Models.Exceptions
{
    public class SelectNotAllowedException : SapphireDbException
    {
        public string ContextName { get; }
        
        public string CollectionName { get; }

        public SelectNotAllowedException(string contextName, string collectionName) : base("Select prefilters are disabled")
        {
            ContextName = contextName;
            CollectionName = collectionName;
        }
    }
}