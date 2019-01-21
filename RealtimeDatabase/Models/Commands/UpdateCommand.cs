using Newtonsoft.Json.Linq;

namespace RealtimeDatabase.Models.Commands
{
    class UpdateCommand : CollectionCommandBase
    {
        public JObject UpdateValue { get; set; }
    }
}
