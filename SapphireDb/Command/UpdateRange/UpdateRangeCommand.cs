using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.UpdateRange
{
    public class UpdateRangeCommand : CollectionCommandBase
    {
        public JArray Values { get; set; }
    }
}
