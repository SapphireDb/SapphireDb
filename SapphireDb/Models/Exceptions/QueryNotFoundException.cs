namespace SapphireDb.Models.Exceptions
{
    public class QueryNotFoundException : SapphireDbException
    {
        public string ContextName { get; set; }
        
        public string CollectionName { get; }

        public string QueryName { get; set; }

        public QueryNotFoundException(string contextName, string collectionName, string queryName) : base("The query was not found")
        {
            ContextName = contextName;
            CollectionName = collectionName;
            QueryName = queryName;
        }
    }
}