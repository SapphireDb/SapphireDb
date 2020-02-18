using Newtonsoft.Json.Linq;

namespace SapphireDb.Command.Execute
{
    public class ExecuteCommand : CommandBase
    {
        public string ActionHandlerName { get; set; }

        public string ActionName { get; set; }

        public JToken[] Parameters { get; set; } = new JToken[0];
    }
}
