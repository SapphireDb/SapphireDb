namespace RealtimeDatabase.Models.Commands
{
    public class ExecuteCommand : CommandBase
    {
        public string ActionHandlerName { get; set; }

        public string ActionName { get; set; }

        public object[] Parameters { get; set; }
    }
}
