using Newtonsoft.Json.Linq;

namespace SapphireDb.Models.Exceptions
{
    public class UpdateRejectedException : SapphireDbException
    {
        public string ContextName { get; }
        
        public string CollectionName { get; }
        
        public JObject OriginalValue { get; }
        
        public JObject UpdatedProperties { get; }
        
        public UpdateRejectedException(string contextName, string collectionName, JObject originalValue, JObject updatedProperties) : base("Update rejected. The object state has changed")
        {
            ContextName = contextName;
            CollectionName = collectionName;
            OriginalValue = originalValue;
            UpdatedProperties = updatedProperties;
        }
    }
}