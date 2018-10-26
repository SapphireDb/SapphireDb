using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Internal
{
    class CommandHandlerMapper
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Dictionary<string, Type> commandHandlerTypes;

        public CommandHandlerMapper(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;

            commandHandlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" && t.Name.EndsWith("Handler"))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler")), t => t);
        }

        public void ExecuteCommand(CommandBase command, DbContextAccesor dbContextAccesor, WebsocketConnection websocketConnection)
        {
            string commandTypeName = command.GetType().Name;

            if (commandHandlerTypes.ContainsKey(commandTypeName))
            {
                Type handlerType = commandHandlerTypes[commandTypeName];

                object handler;

                if (handlerType == typeof(ExecuteCommandHandler))
                {
                    handler = Activator.CreateInstance(handlerType, dbContextAccesor, websocketConnection, serviceProvider);
                }
                else
                {
                    handler = Activator.CreateInstance(handlerType, dbContextAccesor, websocketConnection);
                }


                handlerType.GetMethod("Handle").Invoke(handler, new[] { command });
            }
        }
    }
}
