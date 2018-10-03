using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class UpdateCommand : CollectionCommandBase
    {
        public JObject UpdateValue { get; set; }
    }
}
