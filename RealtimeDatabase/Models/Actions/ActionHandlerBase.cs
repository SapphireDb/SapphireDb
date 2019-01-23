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

        public async Task Notify(object data)
        {
            await websocketConnection.Send(new ExecuteResponse()
            {
                ReferenceId = executeCommand.ReferenceId,
                Result = data,
                Type = ExecuteResponse.ExecuteResponseType.Notify
            });
        }
    }
}
