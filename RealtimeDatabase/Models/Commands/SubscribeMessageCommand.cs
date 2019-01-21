namespace RealtimeDatabase.Models.Commands
{
    class SubscribeMessageCommand : CommandBase
    {
        public string Topic { get; set; }
    }
}
