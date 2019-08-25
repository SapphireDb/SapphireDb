using Microsoft.Extensions.Logging;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;

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

        public void ExecuteCommand(CommandBase command, IServiceProvider serviceProvider, ConnectionBase connection, ILogger<ConnectionBase> logger)
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

                if (!CanExecuteAuthCommand(handlerType, connection, command))
                    return;
            }

            if (handlerType != null && HandleAuthentication(handlerType, connection, command))
            {
                ExecuteAction(handlerType, serviceProvider, command, logger, connection);
            }
        }

        private Dictionary<string, Type> GetHandlerTypes(Type type)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" &&
                t.Name.EndsWith("Handler") && t.IsSubclassOf(type))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal)), t => t);
        }

        private void ExecuteAction(Type handlerType, IServiceProvider serviceProvider, CommandBase command, ILogger<ConnectionBase> logger,
            ConnectionBase connection)
        {
            object handler = serviceProvider.GetService(handlerType);

            if (handler != null)
            {
                logger.LogInformation("Handling " + command.GetType().Name + " with " + handler.GetType().Name);
                Task.Run(async () =>
                {
                    try
                    {
                        if (handler is INeedsConnection handlerWithConnection)
                        {
                            handlerWithConnection.Connection = connection;
                        }

                        ResponseBase response = await (dynamic)handlerType.GetMethod("Handle").Invoke(handler, new object[] { connection.HttpContext, command });

                        if (response != null)
                            _ = connection.Send(response);

                        logger.LogInformation("Handled " + command.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        _ = connection.Send(command.CreateExceptionResponse<ResponseBase>(ex));
                        logger.LogError("Error handling " + command.GetType().Name);
                        logger.LogError(ex.Message);
                    }
                });
            }
            else
            {
                logger.LogError("No handler was found to handle " + command.GetType().Name);
                _ = connection.Send(command.CreateExceptionResponse<ResponseBase>("No handler was found for command"));
            }
        }

        private bool HandleAuthentication(Type handlerType, ConnectionBase connection, CommandBase command)
        {
            if (options.AlwaysRequireAuthentication && handlerType != typeof(LoginCommandHandler) && !connection.HttpContext.User.Identity.IsAuthenticated)
            {
                _ = connection.Send(command.CreateExceptionResponse<ResponseBase>("Authentication required to perform this action"));
                return false;
            }

            return true;
        }

        private bool CanExecuteAuthCommand(Type handlerType, ConnectionBase connection, CommandBase command)
        {
            if (options.EnableAuthCommands && handlerType != typeof(LoginCommandHandler) && handlerType != typeof(RenewCommandHandler))
            {
                if (!connection.HttpContext.User.Identity.IsAuthenticated)
                {
                    _ = connection.Send(command.CreateExceptionResponse<ResponseBase>("User needs authentication to execute auth commands."));
                    return false;
                } 
                else if ((handlerType == typeof(SubscribeUsersCommandHandler) || handlerType == typeof(SubscribeRolesCommandHandler) || handlerType == typeof(QueryConnectionsCommandHandler) || handlerType == typeof(CloseConnectionCommandHandler)) 
                    && !options.AuthInfoAllowFunction(connection.HttpContext))
                {
                    _ = connection.Send(command.CreateExceptionResponse<ResponseBase>("User is not allowed to execute auth info commands."));
                    return false;
                }
                else if (!options.AuthAllowFunction(connection.HttpContext))
                {
                    _ = connection.Send(command.CreateExceptionResponse<ResponseBase>("User is not allowed to execute auth commands."));
                    return false;
                }
            }

            return true;
        }
    }
}
