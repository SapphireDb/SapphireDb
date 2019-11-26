using RealtimeDatabase.Command.Execute;
using RealtimeDatabase.Connection;

namespace RealtimeDatabase.Actions
{
    public class ActionHandlerBase
    {
        public ConnectionBase connection;
        public ExecuteCommand executeCommand;

        public void Notify(object data)
        {
            if (connection != null)
            {
                _ = connection.Send(new ExecuteResponse()
                {
                    ReferenceId = executeCommand.ReferenceId,
                    Result = data,
                    Type = ExecuteResponse.ExecuteResponseType.Notify
                });
            }
        }
    }
}
