namespace RealtimeDatabase.Models.Commands
{
    class PublishCommand : CommandBase
    {
        public string Topic { get; set; }

        public object Data { get; set; }
    }
}
