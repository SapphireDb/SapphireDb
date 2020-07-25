using Newtonsoft.Json.Linq;

namespace SapphireDb.Models.Exceptions
{
    public class UpdateRejectedException : SapphireDbException
    {
        public string CollectionName { get; }
        
        public JObject OriginalValue { get; }
        
        public JObject UpdatedProperties { get; }
        
        public UpdateRejectedException(string collectionName, JObject originalValue, JObject updatedProperties) : base("Update rejected. The object state has changed")
        {
            CollectionName = collectionName;
            OriginalValue = originalValue;
            UpdatedProperties = updatedProperties;
        }
    }
}