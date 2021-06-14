using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.Invoke
{
    public class InvokeCommand: CommandBase
    {
        public Dictionary<string, JValue> PrimaryKeys { get; set; }
        
        public string Action { get; set; }

        public JToken[] Parameters { get; set; } = new JToken[0];
    }
}