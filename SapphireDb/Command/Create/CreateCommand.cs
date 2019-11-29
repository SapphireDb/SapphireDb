using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.Create
{
    public class CreateCommand : CollectionCommandBase
    {
        public JObject Value { get; set; }
    }
}
