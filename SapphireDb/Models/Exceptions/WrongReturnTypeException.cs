namespace SapphireDb.Models.Exceptions
{
    public class WrongReturnTypeException : SapphireDbException
    {
        public string ModelType { get; }
        
        public string MethodName { get; }

        public string ReturnType { get; }

        public WrongReturnTypeException(string modelType, string methodName, string returnType) : base("Wrong return type of method was defined")
        {
            ModelType = modelType;
            MethodName = methodName;
            ReturnType = returnType;
        }
    }
}