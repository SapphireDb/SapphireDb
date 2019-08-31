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
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal
{
    class CommandExecutor
    {
        private readonly RealtimeDatabaseOptions options;
        public readonly Dictionary<string, Type> commandHandlerTypes;
        public readonly Dictionary<string, Type> authCommandHandlerTypes;

        public CommandExecutor(RealtimeDatabaseOptions options)
        {
            this.options = options;

            commandHandlerTypes = GetHandlerTypes(typeof(CommandHandlerBase));
            authCommandHandlerTypes = GetHandlerTypes(typeof(AuthCommandHandlerBase));
        }

        public async Task<ResponseBase> ExecuteCommand<T>(CommandBase command, IServiceProvider serviceProvider, HttpContext context, ILogger<T> logger, ConnectionBase connection = null)
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

                ResponseBase authResponse = CreateAuthResponseOrNull(handlerType, context, command);

                if (authResponse != null)
                {
                    return authResponse;
                }
            }

            if (handlerType != null)
            {
                ResponseBase authResponse = CreateAuthenticationResponseOrNull(handlerType, context, command);

                if (authResponse != null)
                {
                    return authResponse;
                }

                return await ExecuteAction(handlerType, serviceProvider, command, logger, context, connection);
            }

            return null;
        }

        private Dictionary<string, Type> GetHandlerTypes(Type type)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" &&
                t.Name.EndsWith("Handler") && t.IsSubclassOf(type))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal)), t => t);
        }

        private async Task<ResponseBase> ExecuteAction<T>(Type handlerType, IServiceProvider serviceProvider, CommandBase command, ILogger<T> logger,
            HttpContext context, ConnectionBase connection = null)
        {
            object handler = serviceProvider.GetService(handlerType);

            if (handler != null)
            {
                logger.LogInformation("Handling " + command.GetType().Name + " with " + handler.GetType().Name);
                try
                {
                    if (handler is INeedsConnection handlerWithConnection)
                    {
                        if (handler is ExecuteCommandHandler || connection != null)
                        {
                            handlerWithConnection.Connection = connection;
                        }
                        else
                        {
                            logger.LogError("Cannot handle " + command.GetType().Name + " without realtime connection");
                            return command.CreateExceptionResponse<ResponseBase>("Cannot handle this command without realtime connection");
                        }
                    }

                    ResponseBase response = await (dynamic)handlerType.GetMethod("Handle").Invoke(handler, new object[] { context, command });

                    logger.LogInformation("Handled " + command.GetType().Name);

                    if (response?.Error != null)
                    {
                        logger.LogWarning("The handler returned an error for " + command.GetType().Name, response.Error);
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    logger.LogError("Error handling " + command.GetType().Name);
                    logger.LogError(ex.Message);
                    return command.CreateExceptionResponse<ResponseBase>(ex);
                }
            }
            else
            {
                logger.LogError("No handler was found to handle " + command.GetType().Name);
                return command.CreateExceptionResponse<ResponseBase>("No handler was found for command");
            }
        }

        private ResponseBase CreateAuthenticationResponseOrNull(Type handlerType, HttpContext context, CommandBase command)
        {
            if (options.AlwaysRequireAuthentication && handlerType != typeof(LoginCommandHandler) && !context.User.Identity.IsAuthenticated)
            {
                return command.CreateExceptionResponse<ResponseBase>("Authentication required to perform this action");
            }

            return null;
        }

        private ResponseBase CreateAuthResponseOrNull(Type handlerType, HttpContext context, CommandBase command)
        {
            if (options.EnableAuthCommands && handlerType != typeof(LoginCommandHandler) && handlerType != typeof(RenewCommandHandler))
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    return command.CreateExceptionResponse<ResponseBase>("User needs authentication to execute auth commands.");
                } 

                if ((handlerType == typeof(SubscribeUsersCommandHandler) || handlerType == typeof(SubscribeRolesCommandHandler) || handlerType == typeof(QueryConnectionsCommandHandler) || handlerType == typeof(CloseConnectionCommandHandler)) 
                    && !options.AuthInfoAllowFunction(context))
                {
                    return command.CreateExceptionResponse<ResponseBase>("User is not allowed to execute auth info commands.");
                }

                if (!options.AuthAllowFunction(context))
                {
                    return command.CreateExceptionResponse<ResponseBase>("User is not allowed to execute auth commands.");
                }
            }

            return null;
        }
    }
}
