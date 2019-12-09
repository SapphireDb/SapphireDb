using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.CreateRange
{
    public class CreateRangeCommand : CollectionCommandBase
    {
        public JArray Values { get; set; }
    }
}
