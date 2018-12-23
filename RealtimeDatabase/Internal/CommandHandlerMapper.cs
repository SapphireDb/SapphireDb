using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
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
        private readonly RealtimeDatabaseOptions options;
        public readonly Dictionary<string, Type> commandHandlerTypes;
        public readonly Dictionary<string, Type> authCommandHandlerTypes;

        public CommandHandlerMapper(RealtimeDatabaseOptions _options)
        {
            options = _options;

            commandHandlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" && 
                t.Name.EndsWith("Handler") && t.IsSubclassOf(typeof(CommandHandlerBase)))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler")), t => t);

            authCommandHandlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" &&
                t.Name.EndsWith("Handler") && t.IsSubclassOf(typeof(AuthCommandHandlerBase)))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler")), t => t);
        }

        public void ExecuteCommand(CommandBase command, IServiceProvider serviceProvider, WebsocketConnection websocketConnection)
        {
            string commandTypeName = command.GetType().Name;

            Type handlerType = null;

            if (commandHandlerTypes.ContainsKey(commandTypeName))
            {
                handlerType = commandHandlerTypes[commandTypeName];
            }
            else if (authCommandHandlerTypes.ContainsKey(commandTypeName))
            {
                handlerType = authCommandHandlerTypes[commandTypeName];

                if (options.EnableAuthCommands &&
                    handlerType != typeof(LoginCommandHandler) && handlerType != typeof(RenewCommandHandler))
                {
                    if (!websocketConnection.HttpContext.User.Identity.IsAuthenticated)
                    {
                        websocketConnection.Websocket.Send(new ResponseBase()
                        {
                            ReferenceId = command.ReferenceId,
                            Error = new Exception("User needs authentication to execute auth commands.")
                        }).Wait();
                        return;
                    }

                    if (handlerType == typeof(QueryUsersCommandHandler) || handlerType == typeof(QueryRolesCommandHandler))
                    {
                        if (!options.AuthInfoAllowFunction(websocketConnection))
                        {
                            websocketConnection.Websocket.Send(new ResponseBase()
                            {
                                ReferenceId = command.ReferenceId,
                                Error = new Exception("User is not allowed to execute auth info commands.")
                            }).Wait();
                            return;
                        }
                    }
                    else if (!options.AuthAllowFunction(websocketConnection))
                    {
                        websocketConnection.Websocket.Send(new ResponseBase()
                        {
                            ReferenceId = command.ReferenceId,
                            Error = new Exception("User is not allowed to execute auth commands.")
                        }).Wait();
                    }
                }
            }

            if (options.Authentication == RealtimeDatabaseOptions.AuthenticationMode.AlwaysExceptLogin)
            {
                if (handlerType != typeof(LoginCommandHandler) && !websocketConnection.HttpContext.User.Identity.IsAuthenticated)
                {
                    websocketConnection.Websocket.Send(new ResponseBase()
                    {
                        ReferenceId = command.ReferenceId,
                        Error = new Exception("Authentication required to perform this action")
                    }).Wait();
                    return;
                }
            }

            if (handlerType != null)
            {
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
