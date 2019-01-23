using Microsoft.Extensions.Logging;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal
{
    class CommandHandlerMapper
    {
        private readonly RealtimeDatabaseOptions options;
        public readonly Dictionary<string, Type> commandHandlerTypes;
        public readonly Dictionary<string, Type> authCommandHandlerTypes;

        public CommandHandlerMapper(RealtimeDatabaseOptions options)
        {
            this.options = options;

            commandHandlerTypes = GetHandlerTypes(typeof(CommandHandlerBase));
            authCommandHandlerTypes = GetHandlerTypes(typeof(AuthCommandHandlerBase));
        }

        public async Task ExecuteCommand(CommandBase command, IServiceProvider serviceProvider, WebsocketConnection websocketConnection, ILogger<WebsocketConnection> logger)
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

                if (!await CanExecuteAuthCommand(handlerType, websocketConnection, command))
                    return;
            }

            if (handlerType != null && await HandleLoginMode(handlerType, websocketConnection, command))
            {
                ExecuteAction(handlerType, serviceProvider, command, logger, websocketConnection);
            }
        }

        private Dictionary<string, Type> GetHandlerTypes(Type type)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" &&
                t.Name.EndsWith("Handler") && t.IsSubclassOf(type))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal)), t => t);
        }

        private void ExecuteAction(Type handlerType, IServiceProvider serviceProvider, CommandBase command, ILogger<WebsocketConnection> logger,
            WebsocketConnection websocketConnection)
        {
            object handler = serviceProvider.GetService(handlerType);

            logger.LogInformation("Handling " + command.GetType().Name + (handler != null ? " with " + handler.GetType().Name : " failed"));

            if (handler != null)
            {
                new Thread(async () =>
                {
                    try
                    {
                        await (dynamic)handlerType.GetMethod("Handle").Invoke(handler, new object[] { websocketConnection, command });
                        logger.LogInformation("Handled " + command.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        await websocketConnection.SendException<ResponseBase>(command, ex);
                        logger.LogError("Error handling " + command.GetType().Name);
                        logger.LogError(ex.Message);
                    }

                }).Start();
            }
        }

        private async Task<bool> HandleLoginMode(Type handlerType, WebsocketConnection websocketConnection, CommandBase command)
        {
            if (options.Authentication == RealtimeDatabaseOptions.AuthenticationMode.AlwaysExceptLogin)
            {
                if (handlerType != typeof(LoginCommandHandler) && !websocketConnection.HttpContext.User.Identity.IsAuthenticated)
                {
                    await websocketConnection.SendException<ResponseBase>(command,
                        "Authentication required to perform this action");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> CanExecuteAuthCommand(Type handlerType, WebsocketConnection websocketConnection, CommandBase command)
        {
            if (options.EnableAuthCommands && handlerType != typeof(LoginCommandHandler) && handlerType != typeof(RenewCommandHandler))
            {
                if (!websocketConnection.HttpContext.User.Identity.IsAuthenticated)
                {
                    await websocketConnection.SendException<ResponseBase>(command,
                        "User needs authentication to execute auth commands.");
                    return false;
                } 
                else if ((handlerType == typeof(SubscribeUsersCommandHandler) || handlerType == typeof(SubscribeRolesCommandHandler)) 
                    && !options.AuthInfoAllowFunction(websocketConnection))
                {
                    await websocketConnection.SendException<ResponseBase>(command,
                        "User is not allowed to execute auth info commands.");
                    return false;
                }
                else if (!options.AuthAllowFunction(websocketConnection))
                {
                    await websocketConnection.SendException<ResponseBase>(command,
                        "User is not allowed to execute auth commands.");
                    return false;
                }
            }

            return true;
        }
    }
}
