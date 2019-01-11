using Microsoft.Extensions.Logging;
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
using System.Threading.Tasks;

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

                if (options.EnableAuthCommands &&
                    handlerType != typeof(LoginCommandHandler) && handlerType != typeof(RenewCommandHandler))
                {
                    if (!websocketConnection.HttpContext.User.Identity.IsAuthenticated)
                    {
                        await websocketConnection.Send(new ResponseBase()
                        {
                            ReferenceId = command.ReferenceId,
                            Error = new Exception("User needs authentication to execute auth commands.")
                        });
                        return;
                    }

                    if (handlerType == typeof(SubscribeUsersCommandHandler) || handlerType == typeof(SubscribeRolesCommandHandler))
                    {
                        if (!options.AuthInfoAllowFunction(websocketConnection))
                        {
                            await websocketConnection.Send(new ResponseBase()
                            {
                                ReferenceId = command.ReferenceId,
                                Error = new Exception("User is not allowed to execute auth info commands.")
                            });
                            return;
                        }
                    }
                    else if (!options.AuthAllowFunction(websocketConnection))
                    {
                        await websocketConnection.Send(new ResponseBase()
                        {
                            ReferenceId = command.ReferenceId,
                            Error = new Exception("User is not allowed to execute auth commands.")
                        });
                    }
                }
            }

            if (options.Authentication == RealtimeDatabaseOptions.AuthenticationMode.AlwaysExceptLogin)
            {
                if (handlerType != typeof(LoginCommandHandler) && !websocketConnection.HttpContext.User.Identity.IsAuthenticated)
                {
                    await websocketConnection.Send(new ResponseBase()
                    {
                        ReferenceId = command.ReferenceId,
                        Error = new Exception("Authentication required to perform this action")
                    });
                    return;
                }
            }

            if (handlerType != null)
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
                            websocketConnection.Send(new ResponseBase()
                            {
                                ReferenceId = command.ReferenceId,
                                Error = ex
                            }).Wait();

                            logger.LogError("Error handling " + command.GetType().Name);
                            logger.LogError(ex.Message);
                        }
                        
                    }).Start();
                }
            }
        }
    }
}
