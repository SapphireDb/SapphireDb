using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.Execute
{
    public class ExecuteCommand : CommandBase
    {
        public string Action { get; set; }

        public JToken[] Parameters { get; set; } = new JToken[0];
    }
}
