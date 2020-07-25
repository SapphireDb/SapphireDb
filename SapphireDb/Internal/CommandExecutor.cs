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
            Type handlerType = commandHandlerTypes.GetValueOrDefault(commandTypeName);
            
            try
            {
                if (handlerType == null)
                {
                    throw new HandlerNotFoundException();
                }
                
                object handler = serviceProvider.GetService(handlerType);

                if (handler == null)
                {
                    throw new HandlerNotFoundException();
                }

                logger.LogInformation("Handling {command} with {handler}. ConnectionId: {connectionId}",
                    command.GetType().Name, handler.GetType().Name, connection?.Id);

                if (!options.CanExecuteCommand(command, information))
                {
                    throw new UnauthorizedException("You are not allowed to execute this command");
                }

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
                        throw new MissingRealtimeConnectionException();
                    }
                }

                ResponseBase response = await (dynamic) handlerType.GetHandlerHandleMethod()
                    .Invoke(handler, new object[] {information, command});

                logger.LogInformation("Handled {command}. ConnectionId: {connectionId}", command.GetType().Name,
                    connection?.Id);

                return response;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException targetInvocationException &&
                    targetInvocationException.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                if (!(ex is SapphireDbException sapphireDbException))
                {
                    sapphireDbException = new UnhandledException(ex);
                }

                if (sapphireDbException.Severity == ExceptionSeverity.Warning)
                {
                    logger.LogWarning(sapphireDbException,
                        "The command handler return an error with low severity during handling of {command}. Error: {error}, ErrorId: {errorId}, ConnectionId: {connectionId}",
                        command.GetType().Name, sapphireDbException.GetType().Name, sapphireDbException.Id,
                        connection?.Id);
                }
                else
                {
                    logger.LogError(sapphireDbException,
                        "Error handling {command}. Error: {error}, ErrorId: {errorId}, ConnectionId: {connectionId}",
                        command.GetType().Name, sapphireDbException.GetType().Name, sapphireDbException.Id,
                        connection?.Id);
                }

                return new ResponseBase()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new SapphireDbErrorResponse(sapphireDbException)
                };
            }
        }

        private Dictionary<string, Type> GetHandlerTypes(Type type)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Name.EndsWith("Handler") && t.IsSubclassOf(type))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal)),
                    t => t);
        }
    }
}