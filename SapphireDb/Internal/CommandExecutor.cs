using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SapphireDb.Command;
using SapphireDb.Command.Execute;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Internal
{
    class CommandExecutor
    {
        private readonly SapphireDatabaseOptions options;
        public readonly Dictionary<string, Type> commandHandlerTypes;

        public CommandExecutor(SapphireDatabaseOptions options)
        {
            this.options = options;

            commandHandlerTypes = GetHandlerTypes(typeof(CommandHandlerBase));
        }

        public async Task<ResponseBase> ExecuteCommand<T>(CommandBase command, IServiceProvider serviceProvider, HttpInformation information, ILogger<T> logger, ConnectionBase connection = null)
        {
            string commandTypeName = command.GetType().Name;
            
            if (commandHandlerTypes.TryGetValue(commandTypeName, out Type handlerType))
            {
                ResponseBase authResponse = CreateAuthenticationResponseOrNull(handlerType, information, command);

                if (authResponse != null)
                {
                    return authResponse;
                }

                return await ExecuteAction(handlerType, serviceProvider, command, logger, information, connection);
            }

            return null;
        }

        private Dictionary<string, Type> GetHandlerTypes(Type type)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Name.EndsWith("Handler") && t.IsSubclassOf(type))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal)), t => t);
        }

        private async Task<ResponseBase> ExecuteAction<T>(Type handlerType, IServiceProvider serviceProvider, CommandBase command, ILogger<T> logger,
            HttpInformation information, ConnectionBase connection = null)
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

                    ResponseBase response = await (dynamic)handlerType.GetMethod("Handle").Invoke(handler, new object[] { information, command });

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

        private ResponseBase CreateAuthenticationResponseOrNull(Type handlerType, HttpInformation information, CommandBase command)
        {
            if (!options.CanExecuteCommand(command, information))
            {
                return command.CreateExceptionResponse<ResponseBase>("You are not allowed to execute this command");
            }

            return null;
        }
    }
}
