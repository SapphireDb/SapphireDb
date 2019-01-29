namespace RealtimeDatabase.Models.Responses
{
    public class ExecuteResponse : ResponseBase
    {
        public object Result { get; set; }

        public ExecuteResponseType Type { get; set; }

        public enum ExecuteResponseType
        {
            End, Notify
        }
    }
}
