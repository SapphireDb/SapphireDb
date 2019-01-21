using Newtonsoft.Json.Linq;

namespace RealtimeDatabase.Models.Commands
{
    class CreateCommand : CollectionCommandBase
    {
        public JObject Value { get; set; }
    }
}
