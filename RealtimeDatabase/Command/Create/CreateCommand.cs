using Newtonsoft.Json.Linq;

namespace RealtimeDatabase.Command.Create
{
    public class CreateCommand : CollectionCommandBase
    {
        public JObject Value { get; set; }
    }
}
