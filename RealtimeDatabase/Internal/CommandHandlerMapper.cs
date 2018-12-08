using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace RealtimeDatabase.Internal
{
    class CommandHandlerMapper
    {
        public readonly Dictionary<string, Type> commandHandlerTypes;

        public CommandHandlerMapper()
        {
            commandHandlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" && t.Name.EndsWith("Handler"))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler")), t => t);
        }

        public void ExecuteCommand(CommandBase command, IServiceProvider serviceProvider, WebsocketConnection websocketConnection)
        {
            string commandTypeName = command.GetType().Name;

            if (commandHandlerTypes.ContainsKey(commandTypeName))
            {
                Type handlerType = commandHandlerTypes[commandTypeName];
                object handler = serviceProvider.GetService(handlerType);

                if (handler != null)
                {
                    new Thread(() =>
                    {
                        handlerType.GetMethod("Handle").Invoke(handler, new object[] { websocketConnection, command });
                    }).Start();
                }
            }
        }
    }
}
