using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class CreateCommand : CollectionCommandBase
    {
        public JObject Value { get; set; }
    }
}
