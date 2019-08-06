namespace RealtimeDatabase.Models.Commands
{
    public class CollectionCommandBase : CommandBase
    {
        public string CollectionName { get; set; }
        public string ContextName { get; set; } = "Default";
    }
}
