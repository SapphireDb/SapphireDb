namespace SapphireDb.Models.Exceptions
{
    public class NavigationPropertyNotFoundException : SapphireDbException
    {
        public string ModelType { get; }
        
        public string PropertyClassType { get; }
        
        public string Property { get; }
        
        public string Include { get; }

        public NavigationPropertyNotFoundException(string modelType, string propertyClassType, string property, string include) : base("Navigation property not found")
        {
            ModelType = modelType;
            PropertyClassType = propertyClassType;
            Property = property;
            Include = include;
        }
    }
}