namespace SapphireDb.Models.Exceptions
{
    public class OperationDisabledException : SapphireDbException
    {
        public string Operation { get; }
        
        public string ContextName { get; }
        
        public string CollectionName { get; }

        public OperationDisabledException(string operation, string contextName, string collectionName) : base("The operation is disabled")
        {
            Operation = operation;
            ContextName = contextName;
            CollectionName = collectionName;
        }
    }
}