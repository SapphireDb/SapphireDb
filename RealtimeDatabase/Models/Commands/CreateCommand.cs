using Newtonsoft.Json.Linq;

namespace RealtimeDatabase.Models.Commands
{
    public class CreateCommand : CollectionCommandBase
    {
        public JObject Value { get; set; }
    }
}
