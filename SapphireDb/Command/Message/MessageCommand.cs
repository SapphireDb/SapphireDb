namespace SapphireDb.Command.Message
{
    public class MessageCommand : CommandBase
    {
        public object Data { get; set; }
        
        public string Filter { get; set; }
        
        public object[] FilterParameters { get; set; }
    }
}
