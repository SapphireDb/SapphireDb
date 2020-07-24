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
using SapphireDb.Models.Exceptions;

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

        public async Task<ResponseBase> ExecuteCommand<T>(CommandBase command, IServiceProvider serviceProvider,
            HttpInformation information, ILogger<T> logger, ConnectionBase connection = null)
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
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal)),
                    t => t);
        }

        private async Task<ResponseBase> ExecuteAction<T>(Type handlerType, IServiceProvider serviceProvider,
            CommandBase command, ILogger<T> logger,
            HttpInformation information, ConnectionBase connection)
        {
            object handler = serviceProvider.GetService(handlerType);

            if (handler != null)
            {
                logger.LogInformation("Handling {command} with {handler}", command.GetType().Name, handler.GetType().Name);
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
                            logger.LogWarning("Cannot handle {command} without realtime connection",
                                command.GetType().Name);
                            return command.CreateExceptionResponse<ResponseBase>(
                                new MissingRealtimeConnectionException());
                        }
                    }

                    ResponseBase response = await (dynamic) handlerType.GetHandlerHandleMethod()
                        .Invoke(handler, new object[] {information, command});

                    logger.LogInformation("Handled {command}", command.GetType().Name);

                    return response;
                }
                catch (Exception ex)
                {
                    if (ex is TargetInvocationException targetInvocationException && targetInvocationException.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    if (!(ex is SapphireDbException sapphireDbException))
                    {
                        sapphireDbException = new UnhandledException(ex);
                    }
                    
                    if (sapphireDbException.Severity == ExceptionSeverity.Warning)
                    {
                        logger.LogWarning(sapphireDbException, "The command handler return an error with low severity during handling of {command}. Error: {error}, ErrorId: {errorId}", command.GetType().Name, sapphireDbException.GetType().Name, sapphireDbException.Id);
                    }
                    else
                    {
                        logger.LogError(sapphireDbException, "Error handling {command}. Error: {error}, ErrorId: {errorId}", command.GetType().Name, sapphireDbException.GetType().Name, sapphireDbException.Id);
                    }

                    return command.CreateExceptionResponse<ResponseBase>(sapphireDbException);
                }
            }
            else
            {
                logger.LogWarning("No handler was found to handle {command}", command.GetType().Name);
                return command.CreateExceptionResponse<ResponseBase>(new HandlerNotFoundException());
            }
        }

        private ResponseBase CreateAuthenticationResponseOrNull(Type handlerType, HttpInformation information,
            CommandBase command)
        {
            if (!options.CanExecuteCommand(command, information))
            {
                return command.CreateExceptionResponse<ResponseBase>(
                    new UnauthorizedException("You are not allowed to execute this command"));
            }

            return null;
        }
    }
}