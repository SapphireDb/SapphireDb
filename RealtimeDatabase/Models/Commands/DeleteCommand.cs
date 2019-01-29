using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RealtimeDatabase.Models.Commands
{
    public class DeleteCommand : CollectionCommandBase
    {
        public Dictionary<string, JValue> PrimaryKeys { get; set; }
    }
}
