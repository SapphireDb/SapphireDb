using SapphireDb.Command.Execute;
using SapphireDb.Connection;

namespace SapphireDb.Actions
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
        
        public void AsyncResult(object data)
        {
            if (connection != null)
            {
                _ = connection.Send(new ExecuteResponse()
                {
                    ReferenceId = executeCommand.ReferenceId,
                    Result = data,
                    Type = ExecuteResponse.ExecuteResponseType.Async
                });
            }
        }
    }
}
