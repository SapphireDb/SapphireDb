using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.UpdateRange
{
    public class UpdateRangeCommand : CollectionCommandBase
    {
        public List<UpdateEntry> Entries { get; set; }
    }

    public class UpdateEntry
    {
        public JObject Value { get; set; }

        public JObject Previous { get; set; }
    }
}
