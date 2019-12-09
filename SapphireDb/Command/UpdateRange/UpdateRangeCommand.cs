using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.UpdateRange
{
    public class UpdateRangeCommand : CollectionCommandBase
    {
        public JArray UpdateValues { get; set; }
    }
}
