namespace SapphireDb.Models.Exceptions
{
    public class DeleteRejectedException : SapphireDbException
    {
        public string ContextName { get; }
        
        public string Collection { get; }
        
        public object[] PrimaryKey { get; }

        public DeleteRejectedException(string contextName, string collection, object[] primaryKey) : base("Delete rejected. The object state has changed")
        {
            ContextName = contextName;
            Collection = collection;
            PrimaryKey = primaryKey;
        }
    }
}