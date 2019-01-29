namespace RealtimeDatabase.Models.Commands
{
    public class SubscribeMessageCommand : CommandBase
    {
        public string Topic { get; set; }
    }
}
