using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.Delete
{
    public class DeleteCommand : CollectionCommandBase
    {
        public Dictionary<string, JValue> Value { get; set; } = new Dictionary<string, JValue>();
    }
}
