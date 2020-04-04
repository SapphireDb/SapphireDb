namespace SapphireDb.Command.SubscribeMessage
{
    public class TopicResponse : ResponseBase
    {
        public object Message { get; set; }

        public string Topic { get; set; }
    }
}
