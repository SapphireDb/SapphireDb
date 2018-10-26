using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Actions
{
    public class ActionHandlerBase
    {
        public WebsocketConnection WebsocketConnection;
        public ExecuteCommand ExecuteCommand;

        public void Notify(object data)
        {
            WebsocketConnection.Websocket.Send(new ExecuteResponse()
            {
                ReferenceId = ExecuteCommand.ReferenceId,
                Result = data,
                Type = ExecuteResponse.ExecuteResponseType.Notify
            }).Wait();
        }
    }
}
