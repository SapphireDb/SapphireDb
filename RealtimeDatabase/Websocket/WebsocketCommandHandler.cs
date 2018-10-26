using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    class WebsocketCommandHandler
    {
        private readonly DbContextAccesor contextAccesor;
        private readonly CommandHandlerMapper commandHandlerMapper;
        private readonly ActionHandlerAccesor actionHandlerAccesor;

        public WebsocketCommandHandler(DbContextAccesor _contextAccesor, CommandHandlerMapper _commandHandlerMapper, ActionHandlerAccesor _actionHandlerAccesor)
        {
            contextAccesor = _contextAccesor;
            commandHandlerMapper = _commandHandlerMapper;
            actionHandlerAccesor = _actionHandlerAccesor;
        }

        public async Task HandleCommand(WebsocketConnection connection)
        {
            string message = await connection.Websocket.Receive();

            if (!String.IsNullOrEmpty(message))
            {
                CommandBase command = JsonHelper.DeserialzeCommand(message);

                if (command != null)
                {
                    commandHandlerMapper.ExecuteCommand(command, contextAccesor, actionHandlerAccesor, connection);
                }
            }
        }
    }
}
