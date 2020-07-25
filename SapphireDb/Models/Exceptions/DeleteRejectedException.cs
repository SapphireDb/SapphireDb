namespace SapphireDb.Models.Exceptions
{
    public class DeleteRejectedException : SapphireDbException
    {
        public string Collection { get; }
        
        public object[] PrimaryKey { get; }

        public DeleteRejectedException(string collection, object[] primaryKey) : base("Delete rejected. The object state has changed")
        {
            Collection = collection;
            PrimaryKey = primaryKey;
        }
    }
}