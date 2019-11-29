namespace SapphireDb.Command.Publish
{
    public class PublishCommand : CommandBase
    {
        public string Topic { get; set; }

        public object Data { get; set; }
    }
}
