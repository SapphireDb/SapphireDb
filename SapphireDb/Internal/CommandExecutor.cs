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
            HttpInformation information, ConnectionBase connection)
        {
            object handler = serviceProvider.GetService(handlerType);

            if (handler != null)
            {
                logger.LogInformation("Handling {0} with {1}", command.GetType().Name, handler.GetType().Name);
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
                            logger.LogWarning("Cannot handle {0} without realtime connection", command.GetType().Name);
                            return command.CreateExceptionResponse<ResponseBase>("Cannot handle this command without realtime connection");
                        }
                    }
                    
                    ResponseBase response = await (dynamic)handlerType.GetHandlerHandleMethod()
                        .Invoke(handler, new object[] { information, command });

                    logger.LogInformation("Handled {0}", command.GetType().Name);

                    if (response?.Error != null)
                    {
                        logger.LogWarning("The handler returned an error for {0}. Error:\n{1}", command.GetType().Name, response.Error);
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    logger.LogError("Error handling {0}. Error:\n{1}", command.GetType().Name, ex.Message);
                    return command.CreateExceptionResponse<ResponseBase>(ex);
                }
            }
            else
            {
                logger.LogWarning("No handler was found to handle {0}", command.GetType().Name);
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
