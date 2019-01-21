using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Models.Actions
{
    public class ActionHandlerBase
    {
        public WebsocketConnection WebsocketConnection;
        public ExecuteCommand ExecuteCommand;

        public async Task Notify(object data)
        {
            await WebsocketConnection.Send(new ExecuteResponse()
            {
                ReferenceId = ExecuteCommand.ReferenceId,
                Result = data,
                Type = ExecuteResponse.ExecuteResponseType.Notify
            });
        }
    }
}
