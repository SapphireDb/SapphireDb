using Newtonsoft.Json.Linq;

namespace RealtimeDatabase.Models.Commands
{
    public class UpdateCommand : CollectionCommandBase
    {
        public JObject UpdateValue { get; set; }
    }
}
