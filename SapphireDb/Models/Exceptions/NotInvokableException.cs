namespace SapphireDb.Models.Exceptions
{
    public class NotInvokableException : SapphireDbException
    {
        public string ContextName { get; }
        
        public string Collection { get; }
        
        public string Method { get; }

        public NotInvokableException(string contextName, string collection, string method) : base("Method is not invokable")
        {
            ContextName = contextName;
            Collection = collection;
            Method = method;
        }
    }
}