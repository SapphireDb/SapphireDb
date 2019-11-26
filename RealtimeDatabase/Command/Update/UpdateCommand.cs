using Newtonsoft.Json.Linq;

namespace RealtimeDatabase.Command.Update
{
    public class UpdateCommand : CollectionCommandBase
    {
        public JObject UpdateValue { get; set; }
    }
}
