using System;
using SapphireDb.Command.Execute;
using SapphireDb.Connection;

namespace SapphireDb.Actions
{
    public class ActionHandlerBase
    {
        private readonly IServiceProvider _serviceProvider;
        
        public SignalRConnection connection;
        public ExecuteCommand executeCommand;

        public ActionHandlerBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public void Notify(object data)
        {
            if (connection != null)
            {
                _ = connection.Send(new ExecuteResponse()
                {
                    ReferenceId = executeCommand.ReferenceId,
                    Result = data,
                    Type = ExecuteResponse.ExecuteResponseType.Notify
                }, _serviceProvider);
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
                }, _serviceProvider);
            }
        }
    }
}
