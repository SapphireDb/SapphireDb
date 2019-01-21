namespace RealtimeDatabase.Models.Commands
{
    class UnsubscribeMessageCommand : CommandBase
    {
        public string Topic { get; set; }
    }
}
