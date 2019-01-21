using Microsoft.Extensions.Logging;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    class WebsocketCommandHandler
    {
        private readonly CommandHandlerMapper commandHandlerMapper;
        private readonly ILogger<WebsocketConnection> logger;
        private readonly IServiceProvider serviceProvider;

        public WebsocketCommandHandler(IServiceProvider _serviceProvider, CommandHandlerMapper _commandHandlerMapper, ILogger<WebsocketConnection> _logger)
        {
            commandHandlerMapper = _commandHandlerMapper;
            logger = _logger;
            serviceProvider = _serviceProvider;
        }

        public async Task HandleCommand(WebsocketConnection connection)
        {
            string message = await connection.Websocket.Receive();

            if (!String.IsNullOrEmpty(message))
            {
                CommandBase command = JsonHelper.DeserialzeCommand(message);

                if (command != null)
                {
                    await commandHandlerMapper.ExecuteCommand(command, serviceProvider, connection, logger);
                }  
            }
        }
    }
}
