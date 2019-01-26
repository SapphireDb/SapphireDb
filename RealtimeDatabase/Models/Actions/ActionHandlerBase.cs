using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Models.Actions
{
    public class ActionHandlerBase
    {
        public WebsocketConnection websocketConnection;
        public ExecuteCommand executeCommand;

        public void Notify(object data)
        {
            _ = websocketConnection.Send(new ExecuteResponse()
            {
                ReferenceId = executeCommand.ReferenceId,
                Result = data,
                Type = ExecuteResponse.ExecuteResponseType.Notify
            });
        }
    }
}
