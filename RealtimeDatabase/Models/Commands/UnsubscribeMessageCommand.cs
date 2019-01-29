namespace RealtimeDatabase.Models.Commands
{
    public class UnsubscribeMessageCommand : CommandBase
    {
        public string Topic { get; set; }
    }
}
