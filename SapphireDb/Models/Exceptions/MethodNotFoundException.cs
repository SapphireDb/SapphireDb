namespace SapphireDb.Models.Exceptions
{
    public class MethodNotFoundException : SapphireDbException
    {
        public string ModelType { get; }
        
        public string MethodName { get; }

        public MethodNotFoundException(string modelType, string methodName) : base("No suiting method was found")
        {
            ModelType = modelType;
            MethodName = methodName;
        }
    }
}