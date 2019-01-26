using Microsoft.Extensions.Logging;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Threading.Tasks;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Websocket
{
    class WebsocketCommandHandler
    {
        private readonly CommandHandlerMapper commandHandlerMapper;
        private readonly ILogger<WebsocketConnection> logger;
        private readonly IServiceProvider serviceProvider;

        public WebsocketCommandHandler(IServiceProvider serviceProvider, CommandHandlerMapper commandHandlerMapper, ILogger<WebsocketConnection> logger)
        {
            this.commandHandlerMapper = commandHandlerMapper;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task HandleCommand(WebsocketConnection connection)
        {
            string message = await connection.Websocket.Receive();

            if (!string.IsNullOrEmpty(message))
            {
                CommandBase command = JsonHelper.DeserialzeCommand(message);

                if (command != null)
                {
                    commandHandlerMapper.ExecuteCommand(command, serviceProvider, connection, logger);
                }  
            }
        }
    }
}
