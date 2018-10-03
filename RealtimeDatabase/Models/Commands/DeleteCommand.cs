using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class DeleteCommand : CollectionCommandBase
    {
        public Dictionary<string, JValue> PrimaryKeys { get; set; }
    }
}
