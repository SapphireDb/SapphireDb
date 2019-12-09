using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.DeleteRange
{
    public class DeleteRangeCommand : CollectionCommandBase
    {
        public List<Dictionary<string, JValue>> PrimaryKeyList { get; set; } = new List<Dictionary<string, JValue>>();
    }
}
